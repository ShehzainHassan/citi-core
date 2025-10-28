using citi_core.Enums;
using citi_core.Interfaces;
using citi_core.Models;
using Microsoft.EntityFrameworkCore;

namespace citi_core.Data
{
    public class DbOTPRepository : IOTPRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public DbOTPRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddOTPAsync(OTPVerification otp)
        {
            await _dbContext.OTPVerifications.AddAsync(otp);
        }

        public async Task<OTPVerification?> GetLatestOTPAsync(string email, string? phoneNumber, OTPPurpose purpose)
        {
            return await _dbContext.OTPVerifications
                .Where(x => x.Purpose == purpose &&
                            x.Email == email &&
                            x.PhoneNumber == phoneNumber &&
                            !x.IsUsed)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public Task UpdateOTPAsync(OTPVerification otp)
        {
            _dbContext.OTPVerifications.Update(otp);
            return Task.CompletedTask;
        }

        public async Task InvalidateOTPsAsync(string email, string? phoneNumber, OTPPurpose purpose)
        {
            var otps = await _dbContext.OTPVerifications
                .Where(x => x.Purpose == purpose &&
                            x.Email == email &&
                            x.PhoneNumber == phoneNumber &&
                            !x.IsUsed)
                .ToListAsync();

            foreach (var otp in otps)
            {
                otp.IsUsed = true;
                otp.UsedAt = DateTime.UtcNow;
            }
        }
    }
}