using System;
using System.Collections.Generic;
using System.Linq;

namespace Api.Model
{
    public class UserState
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public List<Guid> Roles { get; set; } = new List<Guid>();
        public bool HasRole(Guid role)
        {
            return Roles.Any(x => x == role);
        }
    }
}