using HRSystem.API.DTOs.Auth;

namespace HRSystem.API.Services.Auth;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginDto dto); 
    // no register as users will be created manually. 
}
