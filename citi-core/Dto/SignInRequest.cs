namespace citi_core.Dto
{
    public class SignInRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; } = false;
        public string? DeviceId { get; set; }
        public string? DeviceInfo { get; set; }
    }
}
