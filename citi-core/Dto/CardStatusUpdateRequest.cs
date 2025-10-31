using citi_core.Enums;

namespace citi_core.Dto
{
    public class CardStatusUpdateRequest
    {
        public CardStatus Status { get; set; }
        public string Reason { get; set; } = default!;
    }
}
