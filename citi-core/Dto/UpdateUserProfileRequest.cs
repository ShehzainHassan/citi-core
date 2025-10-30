using System.ComponentModel.DataAnnotations;

namespace citi_core.Dto
{
    public class UpdateUserProfileRequest
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Phone]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }
        public UserPreferencesDto Preferences { get; set; } = new();
    }
}
