namespace Api.Authentication
{
    public sealed class TokenOptions
    {
        public string SecretKey { get; set; }
        public string Audience { get; set; }
        public string Issuer { get; set; }
        public int ExpiresInMinutes { get; set; }
    }
}