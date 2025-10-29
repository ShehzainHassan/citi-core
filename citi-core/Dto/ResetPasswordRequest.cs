namespace citi_core.Dto
{
    public class ResetPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
        public string VerificationToken { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
