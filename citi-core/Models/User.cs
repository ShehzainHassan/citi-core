using citi_core.Enums;
using System.ComponentModel.DataAnnotations;

namespace citi_core.Models
{
    public class User : BaseEntity
    {
        [Key]
        public Guid UserId { get; set; } = Guid.NewGuid();

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        [RegularExpression(@"(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Full Name must be between 2 and 100 characters.")]
        public string FullName { get; set; } = string.Empty;

        [Phone]
        [RegularExpression(@"^\+?[1-9]\d{9,14}$", ErrorMessage = "Phone number must be in valid E.164 format.")]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; } = string.Empty;

        [Required]
        public KycStatus KycStatus { get; set; } = KycStatus.Pending;
        public bool BiometricEnabled { get; set; } = false;
        public bool EmailVerified { get; set; } = false;
        public bool PhoneVerified { get; set; } = false;
        public DateTime? LastLoginAt { get; set; }
        public UserSecuritySettings SecuritySettings { get; set; } = null!;
        public UserPreferences? UserPreferences { get; set; }
        public ICollection<Account> Accounts { get; set; } = new List<Account>();
        public ICollection<Card> Cards { get; set; } = new List<Card>();
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<AuthAuditLog> AuthAuditLogs { get; set; } = new List<AuthAuditLog>();
        public ICollection<CardAuditLog> CardAuditLogs { get; set; } = new List<CardAuditLog>();
        public ICollection<CardRequest> CardRequests { get; set; } = new List<CardRequest>();

    }
}
