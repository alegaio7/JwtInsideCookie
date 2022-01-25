using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Authentication
{
    /// <summary>
    /// A custom middleware used to extract the UserState object inside the received JWT, 
    /// and attach it to the HttpContext
    /// </summary>
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IJwtUtil jwtUtil)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (token != null)
            {
                var userState = jwtUtil.ValidateJwtToken(token);
                if (userState != null)
                {
                    // attach user to context on successful jwt validation
                    context.Items["UserState"] = userState;
                }
            }

            await _next(context);
        }
    }
}