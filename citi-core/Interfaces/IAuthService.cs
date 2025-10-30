using citi_core.Common;
using citi_core.Common.citi_core.Common;
using citi_core.Dto;
using citi_core.Enums;
using citi_core.Models;
using System.Net;
using System.Threading.Tasks;

namespace citi_core.Interfaces
{
    public interface IAuthService
    {
        Task<Result<AuthResponse>> SignUpAsync(SignUpRequest signUpRequest, IPAddress ipAddress, string userAgent);
        Task<Result<bool>> IsEmailAvailableAsync(string email);
        Task<Result<bool>> IsPhoneAvailableAsync(string phoneNumber);
        Task<Result<AuthResponse>> SignInAsync(SignInRequest signInRequest, IPAddress ipAddress, string userAgent);
        Task<Result<AuthResponse>> BiometricSignInAsync(BiometricSignInRequest request, IPAddress ipAddress, string userAgent);
        Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, IPAddress ipAddress);
        Task<Result<bool>> SignOutAsync(string refreshToken, IPAddress ipAddress);
        Task<Result<bool>> ForgotPasswordAsync(ForgotPasswordRequest request);
        Task<Result<string>> VerifyOTPAsync(VerifyOTPRequest request);
        Task<Result<bool>> ResendOTPAsync(ResendOTPRequest request);
        Task<Result<bool>> SetBiometricEnabledAsync(Guid userId, bool enabled);
        Task<Result<bool>> ResetPasswordAsync(ResetPasswordRequest request);
        Task<Result<bool>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
        Task<Result<UserProfileDTO>> GetProfileAsync(Guid userId);
        Task<Result<bool>> UpdateProfileAsync(Guid userId, UpdateUserProfileRequest request);
    }
}
