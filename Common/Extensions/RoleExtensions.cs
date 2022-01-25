using Api.Common.Authorization;
using Api.DTO;
using Api.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;

namespace Api.Common
{
    public static class RoleExtensions
    {
        private static PropertyInfo[] _pis;

        /// <summary>
        /// Converts an Enum of type UserRoles into a Guid, obtained from the type RoleDTO.
        /// The RoleDTO exposes static properties that return a Guid, and they are named the same as the UserRole enum members.
        /// The enum is needed in order to allow the [AuthorizeRole] attribute to work.
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public static Guid RoleTypeToGuid(this UserRoles r)
        {
            if (_pis is null)
                _pis = typeof(RoleDTO).GetProperties(BindingFlags.Public | BindingFlags.Static);
            var pi = _pis.FirstOrDefault(x => x.Name == r.ToString());
            if (pi is null)
                return Guid.Empty;
            else
                return (Guid)pi.GetValue(null); // no obj, reference, since its a static prop.
        }

        public static bool HasRole(this List<Guid> roles, Guid role)
        {
            if (roles is null)
                return false;

            return roles.Any(x => x == role);
        }

        public static bool HasRole(this ClaimsPrincipal cp, Guid role)
        {
            if (cp is null)
                return false;

            var ap = cp as ApiClaimsPrincipal;
            if (ap != null)
                return ap.UserState.Roles.HasRole(role);

            return false;
        }
    }
}