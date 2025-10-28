using citi_core.Enums;
using citi_core.Models;
using System.Text.Json.Serialization;
namespace citi_core.Dto
{
    public class UserProfileDTO
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public KycStatus KycStatus { get; set; }
        public bool BiometricEnabled { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTime? LastLoginAt { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public UserPreferences? Preferences { get; set; }
    }
}
