using Api.Common;
using Api.Common.Authorization;
using Api.Model;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace UI.Authorization
{
    /// <summary>
    /// Extracts the UserState claim from the ClaimsPrincipal created from the cookie, and builds a custom ApiClaimsPrincipal
    /// with the UserState object attached. The claims principal from the httpContext is replaced by this new claims principal.
    /// The UserState object is used to check roles and get other user information.
    /// </summary>
    public class UserStateExtractorMiddleware
    {
        private readonly RequestDelegate _next;

        public UserStateExtractorMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.User?.Identity != null && context.User.Identity.IsAuthenticated && context.User.Claims != null)
            {
                var currentPrincipal = context.User;
                var userStateJson = currentPrincipal.Claims.FirstOrDefault(x => x.Type == "UserState")?.Value;
                UserState userState = default;
                if (!string.IsNullOrEmpty(userStateJson))
                {
                    userState = JsonSerializer.Deserialize<UserState>(userStateJson);
                    if (userState != null)
                    {
                        userState.UserName = currentPrincipal.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)?.Value;
                        userState.Name = currentPrincipal.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Name)?.Value;
                        userState.Email = currentPrincipal.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Email)?.Value;
                        //context.Items["UserState"] = userState;

                        var userIdentity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                        foreach (var c in currentPrincipal.Claims)
                            userIdentity.AddClaim(c);

                        var cp = new ApiClaimsPrincipal(userIdentity);
                        cp.UserState = userState;

                        context.User = cp;
                    }
                }
            }

            await _next(context);
        }
    }
}