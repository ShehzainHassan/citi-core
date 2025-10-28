using citi_core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace citi_core.Models
{
    public class Account : BaseEntity
    {
        [Key]
        public Guid AccountId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string AccountNumber { get; set; } = null!;

        [Required]
        public AccountType AccountType { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AvailableBalance { get; set; }

        [Required]
        [MaxLength(10)]
        public string Currency { get; set; } = "USD";

        [Required]
        [MaxLength(100)]
        public string Branch { get; set; } = null!;

        [Required]
        public AccountStatus Status { get; set; } = AccountStatus.Active;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? InterestRate { get; set; }
        public DateTime? MaturityDate { get; set; }

        [Range(1, 600)]
        public int? TermMonths { get; set; }

        public DateTime OpenedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ClosedAt { get; set; }

        public ICollection<Card> Cards { get; set; } = new List<Card>();
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
