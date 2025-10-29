namespace citi_core.Dto
{
    public class ForgotPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; } = string.Empty;

    }
}
