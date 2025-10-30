using citi_core.Enums;

namespace citi_core.Dto
{
    public class UserPreferencesDto
    {
        public string Language { get; set; } = "en";
        public string Currency { get; set; } = "USD";
        public bool NotificationsEnabled { get; set; }
        public Theme Theme { get; set; } = Theme.Light;

    }
}
