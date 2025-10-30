using citi_core.Models;
using System.Threading.Tasks;

namespace citi_core.Interfaces
{
    public interface IAuthRepository
    {
        Task<User?> GetByIdAsync(Guid userId);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByPhoneAsync(string phoneNumber);
        Task AddUserAsync(User user);
        Task AddAuthLogAsync(AuthAuditLog authLog);
        void UpdateUser(User user);
        Task<User?> GetUserWithPreferencesAsync(Guid userId);
    }
}