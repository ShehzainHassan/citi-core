using citi_core.Models;

namespace citi_core.Interfaces
{
    public interface IAccountRepository
    {
        Task<Account?> GetAccountByIdAsync(Guid accountId);
        Task<List<Account>> GetAllAccountsByUserIdAsync(Guid userId);
        Task<bool> HasActiveCardsAsync(Guid accountId);
        void MarkAccountClosed(Account account);
        Task AddAccountAsync(Account account);
    }
}
