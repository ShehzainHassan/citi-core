using citi_core.Common;
using citi_core.Common.citi_core.Common;
using citi_core.Data;
using citi_core.Dto;
using citi_core.Enums;
using citi_core.Interfaces;
using citi_core.Models;
using citi_core.Utilities;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;

namespace citi_core.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(ITransactionRepository transactionRepository, IUnitOfWork unitOfWork, ILogger<TransactionService> logger)
        {
            _transactionRepository = transactionRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        private async Task<Result<bool>> ValidateTransferAsync(Guid userId, TransferRequest request)
        {
            var sourceAccount = await _unitOfWork.AccountRepository.GetAccountByIdAsync(request.FromAccountId);
            if (sourceAccount == null || sourceAccount.UserId != userId)
                return Result<bool>.Failure("Invalid or unauthorized source account.");

            if (sourceAccount.Status != AccountStatus.Active)
                return Result<bool>.Failure("Source account is inactive.");

            if (sourceAccount.AvailableBalance < request.Amount)
                return Result<bool>.Failure("Insufficient balance.");

            var destinationAccount = await _unitOfWork.AccountRepository.GetAccountByNumberAsync(request.ToAccountNumber);
            if (destinationAccount != null && destinationAccount.AccountId == sourceAccount.AccountId)
                return Result<bool>.Failure("Source and destination accounts must be different.");

            return Result<bool>.Success(true);
        }
        private List<MonthlyChartDto> GenerateChartData(List<Transaction> transactions)
        {
            var now = DateTime.UtcNow;
            var chart = new List<MonthlyChartDto>();

            for (int i = 5; i >= 0; i--)
            {
                var monthStart = new DateTime(now.Year, now.Month, 1).AddMonths(-i);
                var monthEnd = monthStart.AddMonths(1);

                var monthlyTxns = transactions
                    .Where(t => t.TransactionDate >= monthStart && t.TransactionDate < monthEnd)
                    .ToList();

                var income = monthlyTxns.Where(t => t.Amount > 0).Sum(t => t.Amount);
                var expenses = monthlyTxns.Where(t => t.Amount < 0).Sum(t => Math.Abs(t.Amount));
                var net = income - expenses;

                chart.Add(new MonthlyChartDto
                {
                    Month = monthStart.ToString("MMM"),
                    Income = income,
                    Expenses = expenses,
                    Net = net,
                    NormalizedIncome = Normalize(income),
                    NormalizedExpenses = Normalize(expenses),
                    NormalizedNet = Normalize(net)
                });
            }

            return chart;
        }
        private int Normalize(decimal value)
        {
            return (int)Math.Min(100, Math.Round(value / 1000m * 100));
        }
        public async Task<PaginatedResponse<TransactionDto>> GetTransactionsAsync(Guid userId, Guid? accountId, int pageNumber, int pageSize)
        {
            return await _transactionRepository.GetTransactionsAsync(userId, accountId, pageNumber, pageSize);
        }
        public async Task<TransactionDto?> GetTransactionByIdAsync(Guid transactionId, Guid userId)
        {
            return await _transactionRepository.GetTransactionByIdAsync(transactionId, userId);
        }
        public async Task<Result<Guid>> ProcessTransferAsync(Guid userId, TransferRequest request)
        {
            var validation = await ValidateTransferAsync(userId, request);
            if (!validation.IsSuccess)
                return Result<Guid>.Failure(validation.ErrorMessage!);

            var strategy = _unitOfWork.DbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    var sourceAccount = await _unitOfWork.AccountRepository.GetAccountByIdAsync(request.FromAccountId);
                    if (sourceAccount == null || sourceAccount.UserId != userId || sourceAccount.Status != AccountStatus.Active)
                        return Result<Guid>.Failure("Source account not found or inactive");

                    if (sourceAccount.AvailableBalance < request.Amount)
                        return Result<Guid>.Failure("Insufficient balance");

                    var destinationAccount = await _unitOfWork.AccountRepository.GetAccountByNumberAsync(request.ToAccountNumber);
                    if (destinationAccount == null || destinationAccount.Status != AccountStatus.Active)
                        return Result<Guid>.Failure("Destination Account not found or inactive");

                    var debitReference = request.Reference ?? $"TRF{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";
                    var creditReference = $"TRF{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";

                    var debit = new Transaction
                    {
                        TransactionId = Guid.NewGuid(),
                        AccountId = sourceAccount.AccountId,
                        TransactionReference = debitReference,
                        TransactionType = TransactionType.Withdraw,
                        Amount = -request.Amount,
                        BalanceBefore = sourceAccount.Balance,
                        BalanceAfter = sourceAccount.Balance - request.Amount,
                        Currency = request.Currency,
                        Description = request.Description,
                        Status = TransactionStatus.Completed,
                        TransactionDate = DateTime.UtcNow,
                        FromAccount = sourceAccount.AccountNumber,
                        ToAccount = destinationAccount.AccountNumber,
                        BeneficiaryName = request.BeneficiaryName,
                        IsFlagged = false
                    };

                    sourceAccount.Balance -= request.Amount;
                    sourceAccount.AvailableBalance -= request.Amount;

                    await _unitOfWork.TransactionRepository.AddTransactionAsync(debit);

                    if (destinationAccount != null)
                    {
                        var credit = new Transaction
                        {
                            TransactionId = Guid.NewGuid(),
                            AccountId = destinationAccount.AccountId,
                            TransactionReference = creditReference,
                            TransactionType = TransactionType.Transfer,
                            Amount = request.Amount,
                            BalanceBefore = destinationAccount.Balance,
                            BalanceAfter = destinationAccount.Balance + request.Amount,
                            Currency = request.Currency,
                            Description = request.Description,
                            Status = TransactionStatus.Completed,
                            TransactionDate = DateTime.UtcNow,
                            FromAccount = sourceAccount.AccountNumber,
                            ToAccount = destinationAccount.AccountNumber,
                            BeneficiaryName = request.BeneficiaryName,
                            IsFlagged = false
                        };

                        destinationAccount.Balance += request.Amount;
                        destinationAccount.AvailableBalance += request.Amount;

                        await _unitOfWork.TransactionRepository.AddTransactionAsync(credit);
                    }

                    if (request.SaveAsBeneficiary)
                    {
                        var exists = await _unitOfWork.BeneficiaryRepository.ExistsAsync(userId, request.ToAccountNumber);
                        if (!exists)
                        {
                            var beneficiary = new Beneficiary
                            {
                                BeneficiaryId = Guid.NewGuid(),
                                UserId = userId,
                                AccountNumber = request.ToAccountNumber,
                                BeneficiaryName = request.BeneficiaryName,
                                Nickname = request.BeneficiaryNickname
                            };
                            await _unitOfWork.BeneficiaryRepository.AddBeneficiaryAsync(beneficiary);
                        }
                    }

                    await _unitOfWork.TransactionRepository.AddAuditLogAsync(new TransactionAuditLog
                    {
                        AuditLogId = Guid.NewGuid(),
                        UserId = userId,
                        Action = "Transfer-Debit",
                        Reference = debitReference
                    });

                    await _unitOfWork.TransactionRepository.AddAuditLogAsync(new TransactionAuditLog
                    {
                        AuditLogId = Guid.NewGuid(),
                        UserId = userId,
                        Action = "Transfer-Credit",
                        Reference = creditReference
                    });

                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    //TODO: Send Notification to user
                   
                    return Result<Guid>.Success(debit.TransactionId);
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogError(ex, "Transfer failed | UserId: {UserId}", userId);
                    return Result<Guid>.Failure("An error occurred while processing the transfer.");
                }
            });
        }
        public async Task<Result<Guid>> ProcessWithdrawalAsync(Guid userId, WithdrawalRequest request)
        {
            var strategy = _unitOfWork.DbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    var account = await _unitOfWork.AccountRepository.GetAccountByIdAsync(request.FromAccountId);
                    if (account == null || account.UserId != userId || account.Status != AccountStatus.Active)
                        return Result<Guid>.Failure("Account not found or inactive");

                    if (account.AvailableBalance < request.Amount)
                        return Result<Guid>.Failure("Insufficient balance");

                    var reference = request.Reference ?? $"WDL{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";

                    var withdrawal = new Transaction
                    {
                        TransactionId = Guid.NewGuid(),
                        AccountId = account.AccountId,
                        TransactionReference = reference,
                        TransactionType = TransactionType.Withdraw,
                        Amount = -request.Amount,
                        BalanceBefore = account.Balance,
                        BalanceAfter = account.Balance - request.Amount,
                        Currency = request.Currency,
                        Description = request.Description,
                        Status = TransactionStatus.Completed,
                        TransactionDate = DateTime.UtcNow,
                        IsFlagged = false
                    };

                    account.Balance -= request.Amount;
                    account.AvailableBalance -= request.Amount;

                    await _unitOfWork.TransactionRepository.AddTransactionAsync(withdrawal);

                    var auditLog = new TransactionAuditLog
                    {
                        AuditLogId = Guid.NewGuid(),
                        UserId = userId,
                        Action = "Withdrawal",
                        Reference = reference
                    };
                    await _unitOfWork.TransactionRepository.AddAuditLogAsync(auditLog);

                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    // TODO: Send Notification to user

                    return Result<Guid>.Success(withdrawal.TransactionId);
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogError(ex, "Withdrawal failed | UserId: {UserId}", userId);
                    return Result<Guid>.Failure("An error occurred while processing the withdrawal.");
                }
            });
        }
        public async Task<Result<Guid>> ProcessBillPaymentAsync(Guid userId, BillPaymentRequest request)
        {
            var strategy = _unitOfWork.DbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    var account = await _unitOfWork.AccountRepository.GetAccountByIdAsync(request.FromAccountId);
                    if (account == null || account.UserId != userId || account.Status != AccountStatus.Active)
                        return Result<Guid>.Failure("Account not found or inactive");

                    if (account.AvailableBalance < request.Amount)
                        return Result<Guid>.Failure("Insufficient balance");

                    var reference = $"BILL{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";

                    var transaction = new Transaction
                    {
                        TransactionId = Guid.NewGuid(),
                        AccountId = account.AccountId,
                        TransactionReference = reference,
                        TransactionType = TransactionType.BillPayment,
                        Amount = -request.Amount,
                        Category = new TransactionCategory
                        {
                            Name = request.BillType.ToString(),
                            Type = Enums.citi_core.Enums.TransactionCategoryType.Expense,
                            IsSystem = true,
                        },
                        BalanceBefore = account.Balance,
                        BalanceAfter = account.Balance - request.Amount,
                        Currency = request.Currency,
                        Description = request.Description ?? $"Bill payment to {request.BillerName}",
                        Status = TransactionStatus.Completed,
                        TransactionDate = DateTime.UtcNow,
                        MerchantName = request.BillerName,
                        Notes = request.AccountReference,
                        BillType = request.BillType,
                        IsFlagged = false
                    };

                    account.Balance -= request.Amount;
                    account.AvailableBalance -= request.Amount;

                    await _unitOfWork.TransactionRepository.AddTransactionAsync(transaction);

                    var auditLog = new TransactionAuditLog
                    {
                        AuditLogId = Guid.NewGuid(),
                        UserId = userId,
                        Action = "BillPayment",
                        Reference = reference
                    };
                    await _unitOfWork.TransactionRepository.AddAuditLogAsync(auditLog);

                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    // TODO: Send Notification to user

                    return Result<Guid>.Success(transaction.TransactionId);
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogError(ex, "Bill payment failed | UserId: {UserId}", userId);
                    return Result<Guid>.Failure("An error occurred while processing the bill payment.");
                }
            });
        }
        public async Task<Result<Guid>> ProcessDepositAsync(Guid userId, DepositRequest request)
        {
            var strategy = _unitOfWork.DbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    var account = await _unitOfWork.AccountRepository.GetAccountByIdAsync(request.ToAccountId);
                    if (account == null || account.UserId != userId || account.Status != AccountStatus.Active)
                        return Result<Guid>.Failure("Account not found or inactive");

                    var reference = request.Reference ?? $"DEP{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";

                    var deposit = new Transaction
                    {
                        TransactionId = Guid.NewGuid(),
                        AccountId = account.AccountId,
                        TransactionReference = reference,
                        TransactionType = TransactionType.Deposit,
                        Amount = request.Amount,
                        BalanceBefore = account.Balance,
                        BalanceAfter = account.Balance + request.Amount,
                        Currency = request.Currency,
                        Description = request.Description ?? $"Deposit from {request.Source ?? "external source"}",
                        Status = TransactionStatus.Completed,
                        TransactionDate = DateTime.UtcNow,
                        MerchantName = request.Source,
                        IsFlagged = false
                    };

                    account.Balance += request.Amount;
                    account.AvailableBalance += request.Amount;

                    await _unitOfWork.TransactionRepository.AddTransactionAsync(deposit);

                    var auditLog = new TransactionAuditLog
                    {
                        AuditLogId = Guid.NewGuid(),
                        UserId = userId,
                        Action = "Deposit",
                        Reference = reference
                    };
                    await _unitOfWork.TransactionRepository.AddAuditLogAsync(auditLog);

                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    // TODO: Send Notification to user

                    return Result<Guid>.Success(deposit.TransactionId);
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogError(ex, "Deposit failed | UserId: {UserId}", userId);
                    return Result<Guid>.Failure("An error occurred while processing the deposit.");
                }
            });
        }
        public async Task<TransactionReportResponse> GetTransactionReportAsync(Guid userId, TransactionReportRequest request)
        {
            var transactions = await _unitOfWork.TransactionRepository.GetFilteredTransactionAsync(userId, request.StartDate, request.EndDate, request.AccountId, request.CardId, request.TransactionTypes, request.Categories, request.MinAmount, request.MaxAmount, request.Status);

            var grouped = transactions
                .GroupBy(t => DateGroup.GetDateGroup(t.TransactionDate))
                .OrderByDescending(g => g.Key)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(t => new TransactionDto
                    {
                        TransactionId = t.TransactionId,
                        Reference = t.TransactionReference,
                        Type = t.TransactionType,
                        Amount = t.Amount,
                        Description = t.Description,
                        Category = t.Category != null ? new TransactionCategory
                        {
                            TransactionCategoryId = t.Category.TransactionCategoryId,
                            Name = t.Category.Name,
                            Type = t.Category.Type,
                            IsSystem = t.Category.IsSystem
                        } : null,
                        Status = t.Status,
                        TransactionDate = t.TransactionDate,
                        DateGroup = DateGroup.GetDateGroup(t.TransactionDate)
                    }).ToList()
                );

            var summary = new TransactionSummaryDto
            {
                TotalIncome = transactions.Where(t => t.Amount > 0).Sum(t => t.Amount),
                TotalExpenses = transactions.Where(t => t.Amount < 0).Sum(t => Math.Abs(t.Amount)),
                NetAmount = transactions.Sum(t => t.Amount),
                TotalCount = transactions.Count,
                SuccessfulCount = transactions.Count(t => t.Status == TransactionStatus.Completed),
                FailedCount = transactions.Count(t => t.Status == TransactionStatus.Failed)
            };

            var chartData = GenerateChartData(transactions);

            return new TransactionReportResponse
            {
                GroupedTransactions = grouped,
                Summary = summary,
                Chart = chartData
            };
        }
    }
}