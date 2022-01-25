using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace UI.Helpers
{
    public class UnauthorizedJwtRedirectMiddleware
    {
        private readonly RequestDelegate _next;

        public UnauthorizedJwtRedirectMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (UIUnauthorizedException)
            {
                if (context.Response.HasStarted)
                    throw;

                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                context.Response.Redirect("/");
            }
        }
    }
}