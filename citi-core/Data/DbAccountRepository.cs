using citi_core.Data;
using citi_core.Enums;
using citi_core.Interfaces;
using citi_core.Models;
using Microsoft.EntityFrameworkCore;

public class DbAccountRepository : IAccountRepository
{
    private readonly ApplicationDbContext _context;
    public DbAccountRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<Account?> GetAccountByIdAsync(Guid accountId)
    {
        return await _context.Accounts
            .Include(a => a.Cards)
            .Include(a => a.Transactions)
            .FirstOrDefaultAsync(a => a.AccountId == accountId && !a.IsDeleted);
    }
    public async Task<Account?> GetAccountByNumberAsync(string accountNumber)
    {
        return await _context.Accounts
            .Include(a => a.Cards)
            .Include(a => a.Transactions)
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber && !a.IsDeleted);
    }
    public async Task<List<Account>> GetAllAccountsByUserIdAsync(Guid userId)
    {
        return await _context.Accounts
            .Where(a => a.UserId == userId && !a.IsDeleted)
            .AsNoTracking()
            .OrderBy(a => a.OpenedAt)
            .ToListAsync();
    }
    public async Task<bool> HasActiveCardsAsync(Guid accountId)
    {
        return await _context.Cards
            .AnyAsync(c => c.AccountId == accountId && c.Status != CardStatus.Cancelled);
    }
    public void MarkAccountClosed(Account account)
    {
        account.Status = AccountStatus.Closed;
        account.ClosedAt = DateTime.UtcNow;
        account.IsDeleted = true;
    }
    public async Task AddAccountAsync(Account account)
    {
        await _context.Accounts.AddAsync(account);
    }
}