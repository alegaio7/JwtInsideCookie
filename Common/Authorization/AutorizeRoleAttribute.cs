using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Api.Model;
using Api.DTO;
using Api.Common;

namespace Api.Authorization
{
    /// <summary>
    /// Custom authorization attribute that checks for roles in controller action methods
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly IList<UserRoles> _roles;

        public AuthorizeRoleAttribute(params UserRoles[] roles)
        {
            _roles = roles ?? new UserRoles[] { };
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // skip authorization if action is decorated with [AllowAnonymous] attribute
            var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
            if (allowAnonymous)
                return;

            // authorization
            var reject = true;
            var userState = (UserState)context.HttpContext.Items["UserState"];
            // find in the userRoles list
            if (userState != null)
            {
                if (userState.Roles.Any(x => x == RoleDTO.AdministratorRole))
                    reject = false; // don't reject any method if caller is an admin
                else
                {
                    if (userState.Roles.Any(x => _roles.Any(y => y.RoleTypeToGuid() == x)))
                        reject = false;
                }
            }

            if (reject)
                context.Result = new JsonResult(new { message = "User role not authorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
        }
    }
}