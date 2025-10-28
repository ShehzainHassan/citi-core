namespace citi_core.Interfaces
{
    public interface IEmailService
    {
        Task SendOTPEmailAsync(string toEmail, string otpCode);
        Task SendAccountLockoutEmailAsync(string toEmail, string userName);
    }
}