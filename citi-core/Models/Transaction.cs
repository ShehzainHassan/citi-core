using citi_core.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace citi_core.Models
{
    public class Transaction : BaseEntity
    {
        [Key]
        public Guid TransactionId { get; set; } = Guid.NewGuid();
        public Guid? AccountId { get; set; }
        public Guid? CardId { get; set; }
        public Guid? TransactionCategoryId { get; set; }
        public Guid? RecurringScheduleId { get; set; }

        [Required]
        [MaxLength(50)]
        public string TransactionReference { get; set; } = null!;

        [Required]
        public TransactionType TransactionType { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BalanceBefore { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BalanceAfter { get; set; }

        [Required]
        [MaxLength(10)]
        public string Currency { get; set; } = "USD";

        [MaxLength(256)]
        public string? Description { get; set; }

        public TransactionCategory? Category { get; set; }
        public RecurringSchedule? RecurringSchedule { get; set; }

        [Required]
        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

        [MaxLength(50)]
        public string? FromAccount { get; set; }

        [MaxLength(50)]
        public string? ToAccount { get; set; }

        [MaxLength(100)]
        public string? BeneficiaryName { get; set; }

        [Required]
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string? MerchantName { get; set; }

        [MaxLength(100)]
        public string? MerchantCategory { get; set; }

        public BillTypes? BillType { get; set; }

        [MaxLength(200)]
        public string? Location { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        [MaxLength(300)]
        public string? ReceiptUrl { get; set; }
        public bool IsFlagged { get; set; } = false;

        [MaxLength(300)]
        public string? FlagReason { get; set; }
        public Account Account { get; set; } = null!;
        public Card Card { get; set; } = null!;
    }
}