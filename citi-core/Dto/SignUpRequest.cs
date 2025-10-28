using System.ComponentModel.DataAnnotations;

namespace citi_core.Dto
{
    public class SignUpRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool AcceptTerms { get; set; }
        public string? ReferralCode { get; set; }
    }
}
