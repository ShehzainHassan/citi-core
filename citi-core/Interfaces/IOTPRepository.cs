using citi_core.Enums;
using citi_core.Models;
using System.Threading.Tasks;

namespace citi_core.Data
{
    public interface IOTPRepository
    {
        /// <summary>
        /// Adds a new OTP record to the database.
        /// </summary>
        Task AddOTPAsync(OTPVerification otp);

        /// <summary>
        /// Retrieves the latest unused OTP for the given email/phone and purpose.
        /// </summary>
        Task<OTPVerification?> GetLatestOTPAsync(string email, string? phoneNumber, OTPPurpose purpose);

        /// <summary>
        /// Updates an existing OTP record
        /// </summary>
        Task UpdateOTPAsync(OTPVerification otp);

        /// <summary>
        /// Invalidates all unused OTPs for the given email/phone and purpose.
        /// </summary>
        Task InvalidateOTPsAsync(string email, string? phoneNumber, OTPPurpose purpose);

        /// <summary>
        /// Count OTP Request in a time span
        /// </summary>
        Task<int> CountRecentOTPsAsync(string email, string? phoneNumber, OTPPurpose purpose, TimeSpan window);
    }
}
