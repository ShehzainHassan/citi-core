using citi_core.Models;

namespace citi_core.Interfaces
{
    public interface ITransactionRepository
    {
        Task<decimal> GetPendingAmountAsync(Guid accountId);
        Task<bool> HasPendingTransactionsAsync(Guid accountId);
        Task<List<Transaction>> GetStatementAsync(Guid accountId, DateTime start, DateTime end, int skip, int take);
        Task<int> CountStatementAsync(Guid accountId, DateTime start, DateTime end);
    }
}
