using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace citi_core.Models
{
    public class UserSecuritySettings : BaseEntity
    {
        [Key]
        public Guid SecuritySettingsId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public bool TwoFactorEnabled { get; set; } = false;
        public bool BiometricEnabled { get; set; } = false;

        [MaxLength(500)]
        public string? BiometricPublicKey { get; set; }

        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LastFailedLoginAt { get; set; }

        [NotMapped]
        public bool IsLocked => LockedUntil.HasValue && LockedUntil.Value > DateTime.UtcNow;
        public DateTime? LockedUntil { get; set; }
        public DateTime? LastPasswordChangeAt { get; set; }
    }
}
