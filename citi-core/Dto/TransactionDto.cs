using citi_core.Enums;

namespace citi_core.Dto
{
    public class TransactionDto
    {
        public Guid TransactionId { get; set; }
        public DateTime Timestamp { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; } = default!;
        public TransactionStatus Status { get; set; }
    }
}
