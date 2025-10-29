using citi_core.Enums;

namespace citi_core.Dto
{
    public class VerifyOTPRequest
    {
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public OTPPurpose Purpose { get; set; } = OTPPurpose.PasswordReset;
    }
}
