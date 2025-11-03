using citi_core.Enums;
using citi_core.Models;

namespace citi_core.Dto
{
    public class TransactionDto
    {
        public Guid TransactionId { get; set; }
        public string Reference { get; set; } = string.Empty;
        public TransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public string FormattedAmount => $"{(Amount >= 0 ? "+" : "-")}${Math.Abs(Amount):N2}";
        public string? Description { get; set; }
        public TransactionCategory? Category { get; set; }
        public TransactionStatus Status { get; set; }
        public string StatusDisplay => Status == TransactionStatus.Completed ? "Successfully" : "Unsuccessfully";
        public DateTime TransactionDate { get; set; }
        public string DateGroup { get; set; } = string.Empty;
    }
}
