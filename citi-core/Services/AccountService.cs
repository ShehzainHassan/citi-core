using citi_core.Common;
using citi_core.Common.citi_core.Common;
using citi_core.Dto;
using citi_core.Enums;
using citi_core.Interfaces;
using citi_core.Models;
using citi_core.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace citi_core.Services
{
    public class AccountService : IAccountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDistributedCache _cache;
        private readonly ILogger<AccountService> _logger;
        public AccountService(IUnitOfWork unitOfWork, IDistributedCache cache, ILogger<AccountService> logger)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
            _logger = logger;
        }
        public async Task<List<AccountDto>> GetUserAccountsAsync(Guid userId)
        {
            var cacheKey = $"user_accounts_{userId}";
            var cached = await _cache.GetStringAsync(cacheKey);
            if (cached != null)
                return JsonSerializer.Deserialize<List<AccountDto>>(cached)!;

            var accounts = await _unitOfWork.AccountRepository.GetAllAccountsByUserIdAsync(userId);

            var dtos = accounts.Select(a => new AccountDto
            {
                AccountId = a.AccountId,
                MaskedAccountNumber = AccountMaskingHelper.MaskAccountNumber(a.AccountNumber),
                Balance = a.Balance,
                Branch = a.Branch,
                Status = a.Status
            }).ToList();

            var json = JsonSerializer.Serialize(dtos);
            await _cache.SetStringAsync(cacheKey, json, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });

            return dtos;
        }
        public async Task<AccountBalanceDto?> GetAccountBalanceAsync(Guid accountId, Guid userId)
        {
            var cacheKey = $"account_balance_{accountId}";
            var cached = await _cache.GetStringAsync(cacheKey);
            if (cached != null)
                return JsonSerializer.Deserialize<AccountBalanceDto>(cached)!;

            var account = await _unitOfWork.AccountRepository.GetAccountByIdAsync(accountId);
            if (account == null || account.UserId != userId) return null;

            var pendingTotal = await _unitOfWork.TransactionRepository.GetPendingAmountAsync(accountId);

            var dto = new AccountBalanceDto
            {
                Balance = account.Balance,
                AvailableBalance = account.Balance - pendingTotal,
                PendingAmount = pendingTotal
            };

            var json = JsonSerializer.Serialize(dto);
            await _cache.SetStringAsync(cacheKey, json, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
            });

            return dto;
        }
        public async Task<Result<bool>> CloseAccountAsync(Guid accountId, Guid userId)
        {
            var account = await _unitOfWork.AccountRepository.GetAccountByIdAsync(accountId);
            if (account == null || account.UserId != userId)
                return Result<bool>.Failure("Account not found or access denied");

            var hasPending = await _unitOfWork.TransactionRepository.HasPendingTransactionsAsync(accountId);
            if (hasPending)
                return Result<bool>.Failure("Cannot close account with pending transactions");

            var hasActiveCards = await _unitOfWork.AccountRepository.HasActiveCardsAsync(accountId);
            if (hasActiveCards)
                return Result<bool>.Failure("Cancel all linked cards before closing account");

            _unitOfWork.AccountRepository.MarkAccountClosed(account);

            await _unitOfWork.SaveChangesAsync();
            await _cache.RemoveAsync($"user_accounts_{userId}");
            await _cache.RemoveAsync($"account_balance_{accountId}");

            return Result<bool>.Success(true);
        }
        public async Task<AccountDetailsDto?> GetAccountDetailsAsync(Guid accountId, Guid userId)
        {
            var account = await _unitOfWork.AccountRepository.GetAccountByIdAsync(accountId);
            if (account == null || account.UserId != userId) return null;

            var cards = account.Cards.Select(card => new CardDto
            {
                CardId = card.CardId,
                MaskedCardNumber = $"**** **** **** {card.Last4Digits}",
                CardName = card.CardName,
                CardType = card.CardType,
                CardBrand = card.CardBrand,
                AvailableCredit = card.AvailableCredit,
                AccountBalance = account.Balance
            }).ToList();

            var transactions = account.Transactions;

            var totalDeposits = transactions
                .Where(t => t.TransactionType == TransactionType.Deposit && t.Status == TransactionStatus.Completed)
                .Sum(t => t.Amount);

            var totalWithdrawals = transactions
                .Where(t => t.TransactionType == TransactionType.Withdraw && t.Status == TransactionStatus.Completed)
                .Sum(t => t.Amount);

            var completedTransactions = transactions
                .Where(t => t.Status == TransactionStatus.Completed)
                .ToList();

            var averageBalance = completedTransactions.Any()
                ? completedTransactions.Average(t => (t.BalanceBefore + t.BalanceAfter) / 2)
                : account.Balance;


            return new AccountDetailsDto
            {
                AccountId = account.AccountId,
                MaskedAccountNumber = AccountMaskingHelper.MaskAccountNumber(account.AccountNumber),
                Balance = account.Balance,
                Branch = account.Branch,
                Status = account.Status,
                OpenedAt = account.OpenedAt,
                ClosedAt = account.ClosedAt
            };
        }
        public async Task<PaginatedResponse<TransactionDto>> GetAccountStatementAsync(Guid accountId, Guid userId, DateTime startDate, DateTime endDate, int page, int pageSize)
        {
            var account = await _unitOfWork.AccountRepository.GetAccountByIdAsync(accountId);
            if (account == null || account.UserId != userId)
                return new PaginatedResponse<TransactionDto>
                {
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Items = new List<TransactionDto>()
                };

            var skip = (page - 1) * pageSize;
            var transactions = await _unitOfWork.TransactionRepository.GetStatementAsync(accountId, startDate, endDate, skip, pageSize);
            var total = await _unitOfWork.TransactionRepository.CountStatementAsync(accountId, startDate, endDate);

            return new PaginatedResponse<TransactionDto>
            {
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = total,
                Items = transactions.Select(t => new TransactionDto
                {
                    TransactionId = t.TransactionId,
                    Timestamp = t.CreatedAt,
                    Amount = t.Amount,
                    Description = t.Description,
                    Status = t.Status
                }).ToList()
            };
        }
        public async Task<Result<Guid>> AddAccountAsync(Guid userId, CreateAccountRequest request)
        {
            var strategy = _unitOfWork.DbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    var account = new Account
                    {
                        AccountId = Guid.NewGuid(),
                        UserId = userId,
                        AccountNumber = AccountNumberGenerator.Generate(),
                        AccountType = request.AccountType,
                        Branch = request.Branch,
                        Currency = request.Currency ?? "USD",
                        Balance = 0,
                        AvailableBalance = 0,
                        InterestRate = request.InterestRate,
                        TermMonths = request.TermMonths,
                        MaturityDate = request.MaturityDate,
                        Status = AccountStatus.Active,
                        OpenedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.AccountRepository.AddAccountAsync(account);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    await _cache.RemoveAsync($"user_accounts_{userId}");

                    return Result<Guid>.Success(account.AccountId);
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogError(ex, "Failed to create account | UserId: {UserId}", userId);
                    return Result<Guid>.Failure("An error occurred while creating the account.");
                }
            });
        }
    }
}
