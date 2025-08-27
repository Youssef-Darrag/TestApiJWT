namespace TestApiJWT.DTOs
{
    public class AuthDto
    {
        public string Message { get; set; } = default!;
        public bool IsAuthenticated { get; set; }
        public string UserName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public List<string> Roles { get; set; } = default!;
        public string Token { get; set; } = default!;
        public DateTime ExpiresOn { get; set; }
    }
}
