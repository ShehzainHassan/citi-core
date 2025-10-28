using citi_core.Interfaces;
using citi_core.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace citi_core.Data
{
    public class DbAuthRepository : IAuthRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public DbAuthRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbContext.Users
                .Include(u => u.SecuritySettings)
                .Include(u => u.UserPreferences)
                .FirstOrDefaultAsync(u => u.Email == email);
        }
        public async Task<User?> GetByPhoneAsync(string phoneNumber)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        }
        public async Task AddUserAsync(User user)
        {
            await _dbContext.Users.AddAsync(user);
        }
        public async Task AddAuthLogAsync(AuthAuditLog authLog)
        {
            await _dbContext.AuthAuditLogs.AddAsync(authLog);
        }
        public void UpdateUser(User user)
        {
            _dbContext.Users.Update(user);
        }
    }
}