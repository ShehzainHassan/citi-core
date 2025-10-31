using citi_core.Models;

namespace citi_core.Interfaces
{
    public interface ICardRepository
    {
        Task<List<Card>> GetUserCardsAsync(Guid userId);
        Task<Card?> GetCardByIdAsync(Guid cardId, Guid userId);
        Task AddCardAsync(Card card);
        Task AddCardRequestAsync(CardRequest request);
        Task<bool> AccountExistsAsync(Guid accountId, Guid userId);
        Task UpdateAsync(Card card);
        Task LogAuditAsync(CardAuditLog log);
        Task<decimal> GetCardDailyUsageAsync(Guid cardId);
        Task<decimal> GetCardMonthlyUsageAsync(Guid cardId);

    }
}
