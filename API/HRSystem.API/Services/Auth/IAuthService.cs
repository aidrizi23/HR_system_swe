using HRSystem.API.DTOs.Auth;

namespace HRSystem.API.Services.Auth;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<AuthResponseDto> RefreshAsync(string refreshToken);
    Task LogoutAsync(int userId);
    Task ChangePasswordAsync(int userId, ChangePasswordDto dto);
    Task<string?> RequestPasswordResetAsync(string email);
    Task ResetPasswordAsync(ResetPasswordDto dto);
}
