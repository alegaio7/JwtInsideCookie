using Api.Common;
using Api.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace UI.Helpers
{
    public class JwtOnCookieHelper
    {
        public static TokenValidationParameters TokenValidationParameters { get; } = new TokenValidationParameters()
        {
            ValidateIssuerSigningKey = false,
                    ValidateAudience = true,
                    ValidAudience = "http://Api",
                    ValidateIssuer = true,
                    ValidIssuer = "http://Api",
                    ValidateLifetime = true,
                    SignatureValidator = delegate (string token, TokenValidationParameters parameters) // needed since token was created in another service: https://github.com/aspnet/Security/issues/1741
                    {
                        return new JwtSecurityToken(token);
                    }
                };

        public static async Task<UserState> SignInUsingJwt(AuthResult authResult, HttpContext context) {
            // this part validates the token received from the rest Api.
            // we want to store the jwt returned from the service in a cookie for 2 purposes:
            // 1) to identify the client requests (authentication)
            // 2) to evaluate the user roles -encoded in the jwt- when needed (authorization)
            var th = new JwtSecurityTokenHandler();
            th.MapInboundClaims = false;    // this is important to avoid JwtSecurityTokenHandler mapping claims like 'sub' to 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'

            var claimsPrincipal = th.ValidateToken(authResult.token, JwtOnCookieHelper.TokenValidationParameters, out SecurityToken validatedToken);
            if (validatedToken.ValidFrom > DateTime.UtcNow || validatedToken.ValidTo < DateTime.UtcNow)
                throw new Exception("Invalid token");

            // puts the JWT token in an authentication cookie. The cookie will be encrypted using data protection api
            var ap = new AuthenticationProperties() { IsPersistent = true };
            ap.StoreTokens(new[] {
                    new AuthenticationToken() { 
                        Name = JwtAuthTicketFormat.TokenConstants.TokenName, 
                        Value = authResult.token }
                });

            // load UserState (needed to check whether a password change is required)
            var userStateJson = claimsPrincipal.Claims.FirstOrDefault(x => x.Type == "UserState").Value;
            var userState = JsonSerializer.Deserialize<UserState>(userStateJson);

            await context.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                claimsPrincipal,
                ap);

            return userState;
        }
    }
}
