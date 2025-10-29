using citi_core.Models;
using citi_core.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace citi_core.Models
{
    public class OTPVerification : BaseEntity
    {
        [Key]
        public Guid OTPVerificationId { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(150)]
        [EmailAddress] 
        public string? Email { get; set; }

        [MaxLength(20)]
        [Phone]
        public string? PhoneNumber { get; set; }

        [Required]
        [MaxLength(64)]
        public string Code { get; set; } = string.Empty;

        [Required]
        public OTPPurpose Purpose { get; set; }

        [Required]
        public DateTime ExpiresAt { get; set; }

        [NotMapped]
        public bool IsExpired => DateTime.UtcNow > ExpiresAt;

        public bool IsUsed { get; set; } = false;
        public DateTime? UsedAt { get; set; }

        [Range(0, 3)]
        public int AttemptCount { get; set; } = 0;
    }

  
}
