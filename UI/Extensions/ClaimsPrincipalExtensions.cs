using Api.Common.Authorization;
using System.Security.Claims;

namespace UI
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetFullName(this ClaimsPrincipal cp)
        {
            if (cp is null)
                return string.Empty;

            var hrprincipal = cp as ApiClaimsPrincipal;
            if (hrprincipal != null)
                return hrprincipal.UserState.Name;

            return string.Empty;
        }
    }
}