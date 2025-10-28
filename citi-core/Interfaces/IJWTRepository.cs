using citi_core.Models;
using System;
using System.Threading.Tasks;

namespace citi_core.Data
{
    public interface IJWTRepository
    {
        Task AddRefreshTokenAsync(RefreshToken refreshToken);
        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        Task<User?> GetUserByRefreshTokenAsync(string token);
        Task RevokeRefreshTokenAsync(RefreshToken refreshToken, string ipAddress, string? replacementToken = null);
    }
}
