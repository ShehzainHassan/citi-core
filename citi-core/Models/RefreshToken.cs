using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace citi_core.Models
{
    public class RefreshToken : BaseEntity
    {
        [Key]
        public Guid RefreshTokenId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        [Required]
        [MaxLength(500)]
        public string Token { get; set; } = string.Empty;

        [Required]
        public DateTime ExpiresAt { get; set; }

        public bool IsRevoked { get; set; } = false;
        public DateTime? RevokedAt { get; set; }

        [MaxLength(500)]
        public string? ReplacedByToken { get; set; }

        [MaxLength(50)]
        public string? CreatedByIp { get; set; }

        [MaxLength(50)]
        public string? RevokedByIp { get; set; }
    }
}
