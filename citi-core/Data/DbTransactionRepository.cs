using citi_core.Common;
using citi_core.Data;
using citi_core.Dto;
using citi_core.Enums;
using citi_core.Interfaces;
using citi_core.Models;
using citi_core.Utilities;
using Microsoft.EntityFrameworkCore;

public class DbTransactionRepository : ITransactionRepository
{
    private readonly ApplicationDbContext _context;

    public DbTransactionRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<PaginatedResponse<TransactionDto>> GetTransactionsAsync(Guid userId, Guid? accountId, int pageNumber, int pageSize)
    {
        var query = _context.Transactions
            .AsNoTracking()
            .Where(t => t.Account.UserId == userId);

        if (accountId.HasValue)
            query = query.Where(t => t.AccountId == accountId.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(t => t.TransactionDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TransactionDto
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
            })
            .ToListAsync();

        return new PaginatedResponse<TransactionDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
    public async Task<TransactionDto?> GetTransactionByIdAsync(Guid transactionId, Guid userId)
    {
        return await _context.Transactions
            .AsNoTracking()
            .Where(t => t.TransactionId == transactionId && t.Account.UserId == userId)
            .Select(t => new TransactionDto
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
            })
            .FirstOrDefaultAsync();
    }
    public async Task<decimal> GetPendingAmountAsync(Guid accountId)
    {
        return await _context.Transactions
            .Where(t => t.AccountId == accountId && t.Status == TransactionStatus.Pending)
            .SumAsync(t => t.Amount);
    }
    public async Task<bool> HasPendingTransactionsAsync(Guid accountId)
    {
        return await _context.Transactions
            .AnyAsync(t => t.AccountId == accountId && t.Status == TransactionStatus.Pending);
    }
    public async Task<List<Transaction>> GetStatementAsync(Guid accountId, DateTime start, DateTime end, int skip, int take)
    {
        return await _context.Transactions
            .Where(t => t.AccountId == accountId && t.CreatedAt >= start && t.CreatedAt < end)
            .OrderByDescending(t => t.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }
    public async Task<int> CountStatementAsync(Guid accountId, DateTime start, DateTime end)
    {
        return await _context.Transactions
            .CountAsync(t => t.AccountId == accountId && t.CreatedAt >= start && t.CreatedAt < end);
    }
    public async Task AddTransactionAsync(Transaction transaction)
    {
        await _context.Transactions.AddAsync(transaction);
    }
    public async Task AddAuditLogAsync(TransactionAuditLog auditLog)
    {
        await _context.TransactionAuditLogs.AddAsync(auditLog);
    }
    public async Task<List<Transaction>> GetFilteredTransactionAsync(Guid userId, DateTime? startDate, DateTime? endDate, Guid? accountId = null, Guid? cardId = null, List<TransactionType>? transactionTypes = null, List<Guid>? categories = null, decimal? minAmount = null, decimal? maxAmount = null, TransactionStatus? status = null)
    {
        var query = _context.Transactions
            .AsNoTracking()
            .Where(t => t.Account.UserId == userId);

        if (accountId.HasValue)
            query = query.Where(t => t.AccountId == accountId.Value);

        if (cardId.HasValue)
            query = query.Where(t => t.CardId == cardId.Value);

        if (startDate.HasValue)
            query = query.Where(t => t.TransactionDate >= startDate.Value);

        if (endDate.HasValue)
        {
            var inclusiveEnd = endDate.Value.Date.AddDays(1);
            query = query.Where(t => t.TransactionDate < inclusiveEnd);
        }

        if (transactionTypes?.Any() == true)
            query = query.Where(t => transactionTypes.Contains(t.TransactionType));

        if (categories?.Any() == true)
            query = query.Where(t => t.Category != null && categories.Contains(t.Category.TransactionCategoryId));

        if (minAmount.HasValue)
            query = query.Where(t => t.Amount >= minAmount.Value);

        if (maxAmount.HasValue)
            query = query.Where(t => t.Amount <= maxAmount.Value);

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        return await query
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }
}