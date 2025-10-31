using citi_core.Data;
using citi_core.Enums;
using citi_core.Interfaces;
using citi_core.Models;
using Microsoft.EntityFrameworkCore;

public class DbTransactionRepository : ITransactionRepository
{
    private readonly ApplicationDbContext _context;

    public DbTransactionRepository(ApplicationDbContext context)
    {
        _context = context;
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
}