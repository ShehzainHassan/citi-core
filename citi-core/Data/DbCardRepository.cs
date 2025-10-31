using citi_core.Enums;
using citi_core.Interfaces;
using citi_core.Models;
using Microsoft.EntityFrameworkCore;

namespace citi_core.Data
{
    public class DbCardRepository : ICardRepository
    {
        private readonly ApplicationDbContext _context;
        public DbCardRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<Card>> GetUserCardsAsync(Guid userId)
        {
            return await _context.Cards
                .AsNoTracking()
                .Include(c => c.Account)
                .Where(c => c.UserId == userId && c.Status == CardStatus.Active)
                .ToListAsync();
        }
        public async Task<Card?> GetCardByIdAsync(Guid cardId, Guid userId)
        {
            return await _context.Cards
                .AsNoTracking()
                .Include(c => c.Account)
                .FirstOrDefaultAsync(c => c.CardId == cardId && c.UserId == userId);
        }
        public async Task AddCardAsync(Card card)
        {
            await _context.Cards.AddAsync(card);
        }
        public async Task AddCardRequestAsync(CardRequest request)
        {
            await _context.CardRequests.AddAsync(request);
        }
        public async Task<bool> AccountExistsAsync(Guid accountId, Guid userId)
        {
            return await _context.Accounts.AnyAsync(a => a.AccountId == accountId && a.UserId == userId);
        }
        public Task UpdateAsync(Card card)
        {
            _context.Cards.Update(card);
            return Task.CompletedTask;
        }
        public Task LogAuditAsync(CardAuditLog log)
        {
            _context.CardAuditLogs.Add(log);
            return Task.CompletedTask;
        }
        public async Task<decimal> GetCardDailyUsageAsync(Guid cardId)
        {
            var today = DateTime.UtcNow.Date;
            return await _context.Transactions
                .Where(t => t.CardId == cardId && t.CreatedAt.Date == today)
                .SumAsync(t => (decimal?)t.Amount) ?? 0;
        }
        public async Task<decimal> GetCardMonthlyUsageAsync(Guid cardId)
        {
            var now = DateTime.UtcNow;
            return await _context.Transactions
                .Where(t => t.CardId == cardId && t.CreatedAt.Month == now.Month && t.CreatedAt.Year == now.Year)
                .SumAsync(t => (decimal?)t.Amount) ?? 0;
        }
    }
}
