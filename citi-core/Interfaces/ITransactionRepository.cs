using citi_core.Common;
using citi_core.Dto;
using citi_core.Enums;
using citi_core.Models;

namespace citi_core.Interfaces
{
    public interface ITransactionRepository
    {
        Task<PaginatedResponse<TransactionDto>> GetTransactionsAsync(Guid userId, Guid? accountId, int pageNumber, int pageSize);
        Task<TransactionDto?> GetTransactionByIdAsync(Guid transactionId, Guid userId);
        Task<decimal> GetPendingAmountAsync(Guid accountId);
        Task<bool> HasPendingTransactionsAsync(Guid accountId);
        Task<List<Transaction>> GetStatementAsync(Guid accountId, DateTime start, DateTime end, int skip, int take);
        Task<int> CountStatementAsync(Guid accountId, DateTime start, DateTime end);
        Task AddTransactionAsync(Transaction transaction);
        Task AddAuditLogAsync(TransactionAuditLog auditLog);
        Task<List<Transaction>> GetFilteredTransactionAsync(Guid userId, DateTime? startDate, DateTime? endDate, Guid? accountId = null, Guid? cardId = null, List<TransactionType>? transactionTypes = null, List<Guid>? categories = null, decimal? minAmount = null, decimal? maxAmount = null, TransactionStatus? status = null);
    }
}
