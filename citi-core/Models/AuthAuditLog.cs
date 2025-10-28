using citi_core.Models;
using citi_core.Enums;
using System.ComponentModel.DataAnnotations;

namespace citi_core.Models
{
    public class AuthAuditLog : BaseEntity
    {
        [Key]
        public Guid AuthAuditLogId { get; set; } = Guid.NewGuid();

        public Guid? UserId { get; set; }

        [EmailAddress]
        [MaxLength(100)]
        public string? Email { get; set; }

        [Required]
        public AuthActionType ActionType { get; set; }

        [MaxLength(50)]
        public string? IpAddress { get; set; }

        [MaxLength(256)]
        public string? UserAgent { get; set; }

        [MaxLength(100)]
        public string? DeviceId { get; set; }
        public string? AdditionalInfo { get; set; }

        [Required]
        public DateTime ActionDate { get; set; } = DateTime.UtcNow;
        public User? User { get; set; }
    }
}
