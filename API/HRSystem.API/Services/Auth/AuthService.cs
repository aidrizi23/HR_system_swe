using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using HRSystem.API.Data;
using HRSystem.API.DTOs.Auth;
using HRSystem.API.Models.Auth;

namespace HRSystem.API.Services.Auth;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IJwtService _jwtService;

    public AuthService(AppDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user is null || !user.IsActive)
            throw new UnauthorizedAccessException("Invalid email or password.");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await BuildResponseAsync(user);
    }

    public async Task<AuthResponseDto> RefreshAsync(string refreshToken)
    {
        var existing = await _jwtService.ValidateRefreshTokenAsync(refreshToken)
            ?? throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        existing.RevokedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await BuildResponseAsync(existing.User);
    }

    public async Task LogoutAsync(int userId)
    {
        var tokens = await _context.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync();

        foreach (var token in tokens)
            token.RevokedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        var user = await _context.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Current password is incorrect.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _context.SaveChangesAsync();
    }

    public async Task<string?> RequestPasswordResetAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null || !user.IsActive)
            return null;

        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        user.PasswordResetToken = BCrypt.Net.BCrypt.HashPassword(token);
        user.PasswordResetExpiresAt = DateTime.UtcNow.AddHours(1);
        await _context.SaveChangesAsync();

        return token;
    }

    public async Task ResetPasswordAsync(ResetPasswordDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email)
            ?? throw new InvalidOperationException("Invalid or expired reset token.");

        if (user.PasswordResetToken is null || user.PasswordResetExpiresAt is null)
            throw new InvalidOperationException("Invalid or expired reset token.");

        if (user.PasswordResetExpiresAt.Value < DateTime.UtcNow)
            throw new InvalidOperationException("Invalid or expired reset token.");

        if (!BCrypt.Net.BCrypt.Verify(dto.Token, user.PasswordResetToken))
            throw new InvalidOperationException("Invalid or expired reset token.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetExpiresAt = null;
        await _context.SaveChangesAsync();
    }

    private async Task<AuthResponseDto> BuildResponseAsync(User user)
    {
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = await _jwtService.GenerateRefreshTokenAsync(user);

        var minutes = 60;
        return new AuthResponseDto
        {
            Token = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(minutes),
            User = new UserInfoDto
            {
                Id = user.Id,
                Email = user.Email,
                Role = user.Role.ToString(),
                EmployeeId = user.EmployeeId
            }
        };
    }
}
