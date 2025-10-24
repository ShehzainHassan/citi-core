using citi_core.Models;

namespace citi_core.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByPhoneAsync(string phone);
        Task<User> AddUserAsync(User user);
    }
}
