using citi_core.Enums;
using citi_core.Models;

namespace citi_core.Interfaces
{
    public interface IOTPService
    {
        Task<string> GenerateOTPAsync(string email, string? phoneNumber, OTPPurpose purpose);
        Task<bool> VerifyOTPAsync(string email, string? phoneNumber, string code, OTPPurpose purpose);
        Task SendOTPAsync(string email, string? phoneNumber, string code);
        Task InvalidateOTPAsync(string email, string? phoneNumber, OTPPurpose purpose);
    }
}
