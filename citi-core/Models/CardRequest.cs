using citi_core.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace citi_core.Models
{
    public class CardRequest : BaseEntity
    {
        [Key]
        public Guid CardRequestId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid AccountId { get; set; }

        [Required]
        public CardType CardType { get; set; }

        [Required]
        [MaxLength(100)]
        public string CardHolderName { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string CardName { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? DesiredCreditLimit { get; set; }

        [Required]
        public CardStatus Status { get; set; } = CardStatus.Pending;
        public Account Account { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}