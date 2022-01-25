using System;

namespace Api.Model
{
    public class AuthResult
    {
        public string token { get; set; }
        public DateTime expires { get; set; }
    }
}