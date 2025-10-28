using citi_core.Data;
using citi_core.Enums;
using citi_core.Interfaces;
using citi_core.Models;
using System.Security.Cryptography;

namespace citi_core.Services
{
    public class OTPService : IOTPService
    {
        private readonly IOTPRepository _otpRepo;
        private readonly IEmailService _emailService;

        public OTPService(IOTPRepository otpRepo, IEmailService emailService)
        {
            _otpRepo = otpRepo;
            _emailService = emailService;
        }
        public async Task<string> GenerateOTPAsync(string email, string? phoneNumber, OTPPurpose purpose)
        {
            var otp = RandomNumberGenerator.GetInt32(1000, 999999).ToString("D6");

            var otpVerification = new OTPVerification
            {
                Email = email,
                PhoneNumber = phoneNumber,
                Purpose = purpose,
                Code = otp,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                AttemptCount = 0
            };

            await _otpRepo.AddOTPAsync(otpVerification);
            await SendOTPAsync(email, phoneNumber, otp);

            return otp;
        }
        public async Task<bool> VerifyOTPAsync(string email, string? phoneNumber, string code, OTPPurpose purpose)
        {
            var otpEntry = await _otpRepo.GetLatestOTPAsync(email, phoneNumber, purpose);

            if (otpEntry == null || otpEntry.IsExpired || otpEntry.AttemptCount >= 3)
                return false;

            if (otpEntry.Code != code)
            {
                otpEntry.AttemptCount++;
                await _otpRepo.UpdateOTPAsync(otpEntry);
                return false;
            }

            otpEntry.IsUsed = true;
            otpEntry.UsedAt = DateTime.UtcNow;
            await _otpRepo.UpdateOTPAsync(otpEntry);

            return true;
        }
        public async Task SendOTPAsync(string email, string? phoneNumber, string code)
        {
            await _emailService.SendOTPEmailAsync(email, code);
        }
        public async Task InvalidateOTPAsync(string email, string? phoneNumber, OTPPurpose purpose)
        {
            await _otpRepo.InvalidateOTPsAsync(email, phoneNumber, purpose);
        }
    }
}
