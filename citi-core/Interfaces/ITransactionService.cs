using citi_core.Common;
using citi_core.Common.citi_core.Common;
using citi_core.Dto;

namespace citi_core.Interfaces
{
    public interface ITransactionService
    {
        Task<PaginatedResponse<TransactionDto>> GetTransactionsAsync(Guid userId, Guid? accountId, int pageNumber, int pageSize);
        Task<TransactionDto?> GetTransactionByIdAsync(Guid transactionId, Guid userId);
        Task<Result<Guid>> ProcessTransferAsync(Guid userId, TransferRequest request);
        Task<Result<Guid>> ProcessWithdrawalAsync(Guid userId, WithdrawalRequest request);
        Task<Result<Guid>> ProcessBillPaymentAsync(Guid userId, BillPaymentRequest request);
        Task<Result<Guid>> ProcessDepositAsync(Guid userId, DepositRequest request);
        Task<TransactionReportResponse> GetTransactionReportAsync(Guid userId, TransactionReportRequest request);
    }

}
