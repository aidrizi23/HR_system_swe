using HRSystem.API.Models.Auth;

namespace HRSystem.API.Services.Auth;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    Task<RefreshToken> GenerateRefreshTokenAsync(User user);
    Task<RefreshToken?> ValidateRefreshTokenAsync(string token);
}
