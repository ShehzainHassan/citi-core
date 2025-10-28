namespace citi_core.Dto
{
    public class BiometricSignInRequest
    {
        public string Email { get; set; } = string.Empty;
        public string BiometricToken { get; set; } = string.Empty;
        public string? DeviceId { get; set; }
        public string? DeviceInfo { get; set; }
    }
}
