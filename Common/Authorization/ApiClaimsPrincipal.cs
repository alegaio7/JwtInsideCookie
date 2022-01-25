using Api.Model;
using System.Security.Claims;
using System.Security.Principal;

namespace Api.Common.Authorization
{
    public class ApiClaimsPrincipal : ClaimsPrincipal
    {
        public ApiClaimsPrincipal(IIdentity identity) : base(identity)
        {
            //this.Claims = source.Claims;
        }

        public UserState UserState { get; set; }
    }
}