using citi_core.Common;
using citi_core.Common.citi_core.Common;
using citi_core.Dto;

public interface IAccountService
{
    Task<List<AccountDto>> GetUserAccountsAsync(Guid userId);
    Task<AccountDetailsDto?> GetAccountDetailsAsync(Guid accountId, Guid userId);
    Task<AccountBalanceDto?> GetAccountBalanceAsync(Guid accountId, Guid userId);
    Task<PaginatedResponse<TransactionDto>> GetAccountStatementAsync(Guid accountId, Guid userId, DateTime startDate, DateTime endDate, int page, int pageSize);
    Task<Result<bool>> CloseAccountAsync(Guid accountId, Guid userId);
    Task<Result<Guid>> AddAccountAsync(Guid userId, CreateAccountRequest request);
}