using Api.Model;

namespace Api
{
    public static class UserExtensions
    {
        public static string FullName(this User user)
        {
            if (user is null)
                return null;
            return $"{user.FirstName}{(string.IsNullOrEmpty(user.LastName) ? "" : " ")}{user.LastName}";
        }
    }
}