namespace TestApiJWT.DTOs
{
    public class TokenRequestDto
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
    }
}
