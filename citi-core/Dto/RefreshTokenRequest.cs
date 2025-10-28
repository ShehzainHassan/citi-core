namespace citi_core.Dto
{
    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
        public string? DeviceId { get; set; }
    }
}
