using citi_core.Enums;

namespace citi_core.Dto
{
    public class AddCardRequest
    {
        public Guid AccountId { get; set; }
        public CardType CardType { get; set; }
        public string CardHolderName { get; set; } = default!;
        public string CardName { get; set; } = default!;
        public decimal? DesiredCreditLimit { get; set; }
    }
}
