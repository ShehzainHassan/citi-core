using citi_core.Enums;
using System;

namespace citi_core.Models
{
    public class UserPreferences : BaseEntity
    {
        public Guid PreferencesId { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string Language { get; set; } = "en";
        public string Currency { get; set; } = "USD";
        public bool NotificationsEnabled { get; set; }
        public Theme Theme { get; set; } = Theme.Light;
        public User User { get; set; } = null!;
    }
}