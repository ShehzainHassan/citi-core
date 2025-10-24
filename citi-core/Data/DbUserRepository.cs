using citi_core.Interfaces;
using citi_core.Models;
using Microsoft.EntityFrameworkCore;

namespace citi_core.Data
{
    public class DbUserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _db;

        public DbUserRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByPhoneAsync(string phone)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone);
        }

        public async Task<User> AddUserAsync(User user)
        {
            await _db.Users.AddAsync(user);
            return user;
        }
    }
}
