using citi_core.Enums;
using System;
using System.Collections.Generic;

namespace citi_core.Models
{
    public class Account : BaseEntity
    {
        public Guid AccountId { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }

        public string AccountNumber { get; set; } = null!;
        public AccountType AccountType { get; set; }
        public decimal Balance { get; set; }
        public decimal AvailableBalance { get; set; }
        public string Currency { get; set; } = "USD";
        public string Branch { get; set; } = null!;
        public AccountStatus Status { get; set; } = AccountStatus.Active;
        public decimal? InterestRate { get; set; }
        public DateTime? MaturityDate { get; set; }
        public int? TermMonths { get; set; }
        public DateTime OpenedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ClosedAt { get; set; }

        public User User { get; set; } = null!;
        public ICollection<Card> Cards { get; set; } = new List<Card>();
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}