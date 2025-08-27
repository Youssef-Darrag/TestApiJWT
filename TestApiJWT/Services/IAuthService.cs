using TestApiJWT.DTOs;

namespace TestApiJWT.Services
{
    public interface IAuthService
    {
        Task<AuthDto> RegisterAsync(RegisterDto dto);
    }
}
