using citi_core.Enums;

namespace citi_core.Dto
{
    public class BillPaymentRequest
    {
        public Guid FromAccountId { get; set; }
        public BillTypes BillType { get; set; }
        public string BillerName { get; set; } = string.Empty;
        public string Currency { get; set; } = "USD";
        public string AccountReference { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public DateTime? ScheduledDate { get; set; }
    }
}