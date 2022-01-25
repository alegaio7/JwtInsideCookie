using Api.Authentication;
using Api.DTO;
using Api.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private IJwtUtil _jwtUtil;
        private IMemoryCache _cache;

        public AuthController(IJwtUtil jwtUtil)
        {
            _jwtUtil = jwtUtil;
        }

        // GET: api/GetToken
        [HttpGet("gettoken")]
        [ProducesResponseType(StatusCodes.Status200OK),
            ProducesResponseType(StatusCodes.Status404NotFound),
            ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AuthResult>> GetToken(string userName, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(userName))
                    return BadRequest("userName");

                if (string.IsNullOrEmpty(password))
                    return BadRequest("password");

                // use hardcoded users for demo purposes
                // for each user, attach a custom role: A = Admin, E = Employee
                User user = default;
                if (userName == "admin")
                {
                    user = new User() { UserName = "admin", FirstName = "John the admin", LastName = "Doe", Email = "john.doe@company.com", Id = 1 };
                    user.Roles = new List<Guid>();
                    user.Roles.Add(RoleDTO.AdministratorRole);
                }
                else if (userName == "user")
                {
                    user = new User() { UserName = "user", FirstName = "Jane the user", LastName = "Smith", Email = "jane.smith@company.com", Id = 2 };
                    user.Roles = new List<Guid>();
                    user.Roles.Add(RoleDTO.EmployeeRole);
                }
                else
                    return Unauthorized();

                // the method is marked async because building a token usually mean getting info from some DB, which
                // in case of this project, was removed for simplicity reasons. So a fake task is used to keep the method async
                var token = await Task.FromResult(_jwtUtil.GenerateTokenJwt(user));
                return Ok(token);
            }
            catch (Exception)
            {
                // TODO: log the exception
                return Problem();
            }
        }
    }
}