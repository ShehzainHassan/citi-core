using citi_core.Models;
using System.Security.Claims;

namespace citi_core.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        Task SaveRefreshTokenAsync(Guid userId, string token, string ipAddress);
        Task<User?> GetUserByRefreshTokenAsync(string token);
        Task RevokeRefreshTokenAsync(string token, string ipAddress, string? replacementToken = null);
        string GenerateVerificationToken(User user, int expiresInMinutes = 15);
        Task InvalidateAllRefreshTokensAsync(Guid userId);
        ClaimsPrincipal? ValidateVerificationToken(string token);
    }
}
