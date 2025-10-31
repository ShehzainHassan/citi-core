using citi_core.Enums;

namespace citi_core.Dto
{
    public class CardDetailsDto
    {
        public Guid CardId { get; set; }
        public string MaskedCardNumber { get; set; } = default!;
        public string CardHolderName { get; set; } = default!;
        public string CardName { get; set; } = default!;
        public CardType CardType { get; set; }
        public CardBrand CardBrand { get; set; }
        public string ValidFrom { get; set; } = default!;
        public string ExpiryDate { get; set; } = default!;
        public bool IsExpired { get; set; }
        public decimal? AvailableCredit { get; set; }
        public decimal? AccountBalance { get; set; }
        public decimal? DailyLimit { get; set; }
        public decimal? MonthlyLimit { get; set; }
        public decimal? UsedToday { get; set; }
        public decimal? UsedThisMonth { get; set; }
        public AccountDto LinkedAccount { get; set; } = default!;
    }
}
