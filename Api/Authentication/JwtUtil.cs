using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Api.Model;

namespace Api.Authentication
{
    public interface IJwtUtil
    {
        /// <summary>
        /// Builds a JWT that encodes a user state object with custom properties needed by the solution,
        /// and also encodes standard claims (like name, email).
        /// The method returns an AuthResult object which contains the token and its expiration time.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        AuthResult GenerateTokenJwt(User user);

        /// <summary>
        /// Validates a JWT and if successful, returns the UserState custom claim which contains user-specific properties.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        UserState ValidateJwtToken(string token);
    }

    /// <summary>
    /// JWT Token generator class using "secret-key"
    /// more info: https://self-issued.info/docs/draft-ietf-oauth-json-web-token.html
    /// </summary>
    public class JwtUtil : IJwtUtil
    {
        private IOptions<TokenOptions> _jwtOptions;

        public JwtUtil(IOptions<TokenOptions> options)
        {
            _jwtOptions = options;
        }

        public AuthResult GenerateTokenJwt(User user)
        {
            if (user is null)
                throw new NullReferenceException($"User is null in {nameof(GenerateTokenJwt)}");

            var securityKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_jwtOptions.Value.SecretKey));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            // the claims we need to encode in the JWT:
            var claims = new List<Claim>(new[] {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // make token unique
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Name, user.FullName())
            });

            // UserState is a custom object that will also be encoded inside a claim, inside the JWT.
            var u = new UserState();
            u.UserId = user.Id;
            // these 3 props aren't encoded in UserState when SENDING the jwt, since the info already exists in standard claims (see above)
            // but the props exists in the UserState class in order to fill them when a jwt is RECEIVED. See ValidateJwtToken
            // u.UserName = user.Login.UserName;    // don't encode this in UserState, since its part of the claims
            // u.Name = user.Name                   // don't encode this in UserState, since its part of the claims
            // u.Email = user.Email;                // don't encode this in UserState, since its part of the claims

            var cleanList = new List<Guid>();
            // loop to avoid adding the same role twice, otherwise the JWT will increase in size unnecesarily
            foreach (var r in user.Roles)
            {
                if (!cleanList.Contains(r))
                    cleanList.Add(r);
            }
            u.Roles = cleanList;
            claims.Add(new Claim("UserState", JsonSerializer.Serialize(u)));

            // Build the security token
            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwtOptions.Value.Issuer,
                audience: _jwtOptions.Value.Audience,
                expires: DateTime.UtcNow.AddMinutes(_jwtOptions.Value.ExpiresInMinutes),
                claims: claims,
                signingCredentials: signingCredentials
            );

            // Write the security token in string format
            var jwtTokenString = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

            // Returns a custom object containing the token and its expiration time
            return new AuthResult { token = jwtTokenString, expires = jwtSecurityToken.ValidTo };
        }

        public UserState ValidateJwtToken(string token)
        {
            if (token == null)
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtOptions.Value.SecretKey);

            try
            {
                // Validates the token in string format, and if successful, a SecurityToken is created.
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;

                // Finds the 'UserState' custom claim inside the token
                var userStateJson = jwtToken.Claims.FirstOrDefault(x => x.Type == "UserState")?.Value;

                UserState userState = default;
                if (!string.IsNullOrEmpty(userStateJson))
                {
                    // Converts the claim (string) into a UserState object.
                    userState = JsonSerializer.Deserialize<UserState>(userStateJson);
                    if (userState != null)
                    {
                        // Fills other UserState properties that are encoded in stardard claims
                        userState.UserName = jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)?.Value;
                        userState.Name = jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Name)?.Value;
                        userState.Email = jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Email)?.Value;
                    }
                }
                // return user id from JWT token if validation successful
                return userState;
            }
            catch
            {
                // return null if validation fails
                return null;
            }
        }
    }
}