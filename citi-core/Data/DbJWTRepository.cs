using citi_core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace citi_core.Data
{
    public class DbJWTRepository : IJWTRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public DbJWTRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddRefreshTokenAsync(RefreshToken refreshToken)
        {
            await _dbContext.RefreshTokens.AddAsync(refreshToken);
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
        {
            return await _dbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task<User?> GetUserByRefreshTokenAsync(string token)
        {
            var refreshToken = await _dbContext.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow);

            return refreshToken?.User;
        }

        public Task RevokeRefreshTokenAsync(RefreshToken refreshToken, string ipAddress, string? replacementToken = null)
        {
            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;

            if (!string.IsNullOrEmpty(replacementToken))
            {
                refreshToken.ReplacedByToken = replacementToken;
            }

            _dbContext.RefreshTokens.Update(refreshToken);
            return Task.CompletedTask;
        }
    }
}
