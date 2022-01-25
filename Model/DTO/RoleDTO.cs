using System;

namespace Api.DTO
{
    public class RoleDTO
    {
        public const string ROLE_ADMIN = "8F31D2F3-78C1-49A4-9858-C87837A460D3";
        public const string ROLE_EMPLOYEE = "38E562F0-9B43-4D05-BE48-0A8D23EA521A";

        public static Guid AdministratorRole
        {
            get
            {
                return Guid.Parse(ROLE_ADMIN);
            }
        }

        public static Guid EmployeeRole
        {
            get
            {
                return Guid.Parse(ROLE_EMPLOYEE);
            }
        }

        public int Id { get; set; }
        public string Name { get; set; }
    }
}