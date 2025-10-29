using citi_core.Data;
using citi_core.Enums;
using citi_core.Interfaces;
using citi_core.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace citi_core.Services
{
    public class OTPService : IOTPService
    {
        private readonly IOTPRepository _otpRepo;
        private readonly IEmailService _emailService;
        private readonly string _otpSecretKey;

        public OTPService(IOTPRepository otpRepo, IEmailService emailService, IConfiguration config)
        {
            _otpRepo = otpRepo;
            _emailService = emailService;
            _otpSecretKey = config["OTP:SecretKey"] ?? throw new InvalidOperationException("Missing OTP secret key");
        }
        private string HashOtp(string otp)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_otpSecretKey));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(otp));
            return Convert.ToBase64String(hash);
        }
        public async Task<string> GenerateOTPAsync(string email, string? phoneNumber, OTPPurpose purpose)
        {
            var recentCount = await _otpRepo.CountRecentOTPsAsync(email, phoneNumber, purpose, TimeSpan.FromMinutes(10));
            if (recentCount >= 3)
                throw new InvalidOperationException("Too many OTP requests. Please wait before trying again");

            var otp = RandomNumberGenerator.GetInt32(1000, 999999).ToString();
            var hashedOtp = HashOtp(otp);

            var otpVerification = new OTPVerification
            {
                Email = email,
                PhoneNumber = phoneNumber,
                Purpose = purpose,
                Code = hashedOtp,
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

            var hashedInput = HashOtp(code);

            if (otpEntry.Code != hashedInput)
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
