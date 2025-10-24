using citi_core.Enums;
using System;

namespace citi_core.Models
{
    public class Transaction : BaseEntity
    {
        public Guid TransactionId { get; set; } = Guid.NewGuid();
        public Guid? AccountId { get; set; }
        public Guid? CardId { get; set; }

        public string TransactionReference { get; set; } = null!;
        public TransactionType TransactionType { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }
        public string? Description { get; set; }
        public TransactionCategory? Category { get; set; }
        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
        public string? FromAccount { get; set; }
        public string? ToAccount { get; set; }
        public string? BeneficiaryName { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        public Account Account { get; set; } = null!;
        public Card Card { get; set; } = null!;
    }
}