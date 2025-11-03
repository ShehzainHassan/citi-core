using System.ComponentModel.DataAnnotations;

namespace citi_core.Models
{
    public class Beneficiary : BaseEntity
    {
        [Key]
        public Guid BeneficiaryId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string BeneficiaryName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string AccountNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string BankName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string BankCode { get; set; } = string.Empty;

        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(50)]
        public string? Nickname { get; set; } = string.Empty;

        public bool IsFavorite { get; set; }
        public User User { get; set; } = null!;
    }
}
