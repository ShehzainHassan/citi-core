using citi_core.Enums;
using System;
using System.Collections.Generic;

namespace citi_core.Models
{
    public class Card : BaseEntity
    {
        public Guid CardId { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public Guid AccountId { get; set; }

        public string CardNumber { get; set; } = null!;
        public string CVV { get; set; } = null!;
        public string Last4Digits { get; set; } = null!;
        public string CardHolderName { get; set; } = null!;
        public string CardName { get; set; } = null!;
        public CardType CardType { get; set; }
        public CardBrand CardBrand { get; set; }
        public string ValidFrom { get; set; } = null!;
        public string ExpiryDate { get; set; } = null!;
        public decimal? CreditLimit { get; set; }
        public decimal? AvailableCredit { get; set; }
        public CardStatus Status { get; set; } = CardStatus.Active;
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
        public Account Account { get; set; } = null!;
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}