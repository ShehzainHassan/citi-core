using citi_core.Enums;

namespace citi_core.Dto
{
    public class CardDto
    {
        public Guid CardId { get; set; }
        public string MaskedCardNumber { get; set; } = default!;
        public string CardName { get; set; } = default!;
        public CardType CardType { get; set; }
        public CardBrand CardBrand { get; set; }
        public decimal? AvailableCredit { get; set; }
        public decimal? AccountBalance { get; set; }
    }
}
