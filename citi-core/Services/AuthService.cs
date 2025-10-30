using BCrypt.Net;
using citi_core.Common.citi_core.Common;
using citi_core.Dto;
using citi_core.Enums;
using citi_core.Interfaces;
using citi_core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace citi_core.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOTPService _otpService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthService> _logger;
        private const int LockoutThreshold = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(30);
        public AuthService(IUnitOfWork unitOfWork, IOTPService otpService, IJwtTokenService jwtTokenService, IEmailService emailService, ILogger<AuthService> logger)
        {
            _unitOfWork = unitOfWork;
            _otpService = otpService;
            _jwtTokenService = jwtTokenService;
            _emailService = emailService;
            _logger = logger;
        }
        public async Task<Result<AuthResponse>> SignUpAsync(SignUpRequest signUpRequest, IPAddress ipAddress, string userAgent)
        {
            var strategy = _unitOfWork.DbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    var existingEmail = await _unitOfWork.AuthRepository.GetByEmailAsync(signUpRequest.Email);
                    if (existingEmail != null)
                        return Result<AuthResponse>.Failure("Email is already taken.");

                    if (!string.IsNullOrWhiteSpace(signUpRequest.PhoneNumber))
                    {
                        var existingPhone = await _unitOfWork.AuthRepository.GetByPhoneAsync(signUpRequest.PhoneNumber);
                        if (existingPhone != null)
                            return Result<AuthResponse>.Failure("Phone number is already taken.");
                    }

                    if (!signUpRequest.AcceptTerms)
                        return Result<AuthResponse>.Failure("Terms must be accepted.");

                    var passwordHash = BCrypt.Net.BCrypt.HashPassword(signUpRequest.Password, workFactor: 12);

                    var user = new User
                    {
                        FullName = signUpRequest.FullName,
                        Email = signUpRequest.Email,
                        PhoneNumber = signUpRequest.PhoneNumber,
                        PasswordHash = passwordHash,
                        KycStatus = KycStatus.Pending,
                        BiometricEnabled = false,
                        EmailVerified = false,
                        PhoneVerified = false,
                        UserPreferences = new UserPreferences
                        {
                            Language = "en",
                            Currency = "USD",
                            Theme = Theme.Light
                        },
                        SecuritySettings = new UserSecuritySettings
                        {
                            TwoFactorEnabled = false,
                            BiometricEnabled = false,
                            FailedLoginAttempts = 0,
                            LastPasswordChangeAt = DateTime.UtcNow
                        }
                    };

                    user.LastLoginAt = DateTime.UtcNow;
                    await _unitOfWork.DbContext.Users.AddAsync(user);
                    await _unitOfWork.SaveChangesAsync();

                    //await _otpService.GenerateOTPAsync(
                    //    email: signUpRequest.Email,
                    //    phoneNumber: string.IsNullOrWhiteSpace(signUpRequest.PhoneNumber) ? null : signUpRequest.PhoneNumber,
                    //    purpose: OTPPurpose.Registration);

                    var accessToken = _jwtTokenService.GenerateAccessToken(user);
                    var refreshToken = _jwtTokenService.GenerateRefreshToken();
                    await _jwtTokenService.SaveRefreshTokenAsync(user.UserId, refreshToken, ipAddress.ToString());

                    var authLog = new AuthAuditLog
                    {
                        UserId = user.UserId,
                        Email = user.Email,
                        ActionType = AuthActionType.LoginSuccess,
                        IpAddress = ipAddress.ToString(),
                        UserAgent = userAgent,
                        ActionDate = DateTime.UtcNow,
                    };

                    await _unitOfWork.DbContext.AuthAuditLogs.AddAsync(authLog);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    var authResponse = new AuthResponse
                    {
                        AccessToken = accessToken,
                        RefreshToken = refreshToken,
                        ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                        UserProfile = new UserProfileDTO
                        {
                            UserId = user.UserId,
                            Email = user.Email,
                            FullName = user.FullName,
                            PhoneNumber = user.PhoneNumber ?? "",
                            KycStatus = user.KycStatus,
                            BiometricEnabled = user.BiometricEnabled,
                            TwoFactorEnabled = user.SecuritySettings?.TwoFactorEnabled ?? false,
                            LastLoginAt = user.LastLoginAt,
                            Preferences = new UserPreferencesDto
                            {
                                Language = user.UserPreferences?.Language ?? "en",
                                Currency = user.UserPreferences?.Currency ?? "USD",
                                Theme = user.UserPreferences?.Theme ?? Theme.Light,
                                NotificationsEnabled = user.UserPreferences?.NotificationsEnabled ?? true
                            }
                        }
                    };

                    return Result<AuthResponse>.Success(authResponse);
                }
                catch (Exception)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result<AuthResponse>.Failure("An error occurred during registration.");
                }
            });
        }
        public async Task<Result<bool>> IsEmailAvailableAsync(string email)
        {
            var exists = await _unitOfWork.DbContext.Users.AnyAsync(u => u.Email == email);
            return exists ? Result<bool>.Failure("Email is already taken.") : Result<bool>.Success(true);
        }
        public async Task<Result<bool>> IsPhoneAvailableAsync(string phoneNumber)
        {
            var exists = await _unitOfWork.DbContext.Users.AnyAsync(u => u.PhoneNumber == phoneNumber);
            return exists ? Result<bool>.Failure("Phone number is already taken.") : Result<bool>.Success(true);
        }
        public async Task<Result<AuthResponse>> SignInAsync(SignInRequest signInRequest, IPAddress ipAddress, string userAgent)
        {
            var strategy = _unitOfWork.DbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    var user = await _unitOfWork.AuthRepository.GetByEmailAsync(signInRequest.Email);
                    if (user == null)
                        return Result<AuthResponse>.Failure("Invalid credentials.");

                    if (user.IsDeleted)
                        return Result<AuthResponse>.Failure("Account not found.");

                    var settings = user.SecuritySettings ?? new UserSecuritySettings();

                    if (settings.LockedUntil.HasValue && settings.LockedUntil.Value > DateTime.UtcNow)
                        return Result<AuthResponse>.Failure("Account is locked. Please try again later.");

                    var passwordValid = BCrypt.Net.BCrypt.Verify(signInRequest.Password, user.PasswordHash);
                    if (!passwordValid)
                    {
                        settings.FailedLoginAttempts += 1;
                        settings.LastFailedLoginAt = DateTime.UtcNow;

                        if (settings.FailedLoginAttempts >= LockoutThreshold)
                        {
                            settings.LockedUntil = DateTime.UtcNow.Add(LockoutDuration);
                            await _emailService.SendAccountLockoutEmailAsync(user.Email, user.FullName);
                        }

                        _unitOfWork.AuthRepository.UpdateUser(user);
                        await _unitOfWork.SaveChangesAsync();

                        var failedLog = new AuthAuditLog
                        {
                            UserId = user.UserId,
                            Email = user.Email,
                            ActionType = AuthActionType.LoginFailed,
                            IpAddress = ipAddress.ToString(),
                            UserAgent = userAgent,
                            ActionDate = DateTime.UtcNow,
                            DeviceId = signInRequest.DeviceId,
                            AdditionalInfo = signInRequest.DeviceInfo
                        };

                        await _unitOfWork.AuthRepository.AddAuthLogAsync(failedLog);
                        await _unitOfWork.SaveChangesAsync();
                        await _unitOfWork.CommitTransactionAsync();

                        return Result<AuthResponse>.Failure("Invalid credentials.");
                    }

                    user.SecuritySettings ??= new UserSecuritySettings();
                    user.SecuritySettings.FailedLoginAttempts = 0;
                    user.SecuritySettings.LockedUntil = null;
                    user.LastLoginAt = DateTime.UtcNow;

                    _unitOfWork.AuthRepository.UpdateUser(user);
                    await _unitOfWork.SaveChangesAsync();

                    var accessToken = _jwtTokenService.GenerateAccessToken(user);
                    var refreshToken = _jwtTokenService.GenerateRefreshToken();
                    await _jwtTokenService.SaveRefreshTokenAsync(user.UserId, refreshToken, ipAddress.ToString());

                    var successLog = new AuthAuditLog
                    {
                        UserId = user.UserId,
                        Email = user.Email,
                        ActionType = AuthActionType.LoginSuccess,
                        IpAddress = ipAddress.ToString(),
                        UserAgent = userAgent,
                        ActionDate = DateTime.UtcNow,
                        DeviceId = signInRequest.DeviceId,
                        AdditionalInfo = signInRequest.DeviceInfo
                    };

                    await _unitOfWork.AuthRepository.AddAuthLogAsync(successLog);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    var authResponse = new AuthResponse
                    {
                        AccessToken = accessToken,
                        RefreshToken = refreshToken,
                        ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                        UserProfile = new UserProfileDTO
                        {
                            UserId = user.UserId,
                            Email = user.Email,
                            FullName = user.FullName,
                            PhoneNumber = user.PhoneNumber ?? "",
                            KycStatus = user.KycStatus,
                            BiometricEnabled = user.BiometricEnabled,
                            TwoFactorEnabled = user.SecuritySettings?.TwoFactorEnabled ?? false,
                            LastLoginAt = user.LastLoginAt,
                            Preferences = new UserPreferencesDto
                            {
                                Language = user.UserPreferences?.Language ?? "en",
                                Currency = user.UserPreferences?.Currency ?? "USD",
                                Theme = user.UserPreferences?.Theme ?? Theme.Light,
                                NotificationsEnabled = user.UserPreferences?.NotificationsEnabled ?? true
                            }
                        }
                    };

                    return Result<AuthResponse>.Success(authResponse);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result<AuthResponse>.Failure("An error occurred during sign-in.");
                }
            });
        }
        public async Task<Result<AuthResponse>> BiometricSignInAsync(BiometricSignInRequest request, IPAddress ipAddress, string userAgent)
        {
            var user = await _unitOfWork.AuthRepository.GetByEmailAsync(request.Email);
            if (user == null) return Result<AuthResponse>.Failure("Invalid biometric login.");

            var settings = user.SecuritySettings ?? (user.SecuritySettings = new UserSecuritySettings());
            if (!user.SecuritySettings?.BiometricEnabled ?? true)
                return Result<AuthResponse>.Failure("Biometric sign-in not enabled for user.");

            // TODO: Replace with real biometric token verification.
            var biometricValid = !string.IsNullOrWhiteSpace(request.BiometricToken);
            if (!biometricValid)
            {
                return Result<AuthResponse>.Failure("Invalid biometric token.");
            }

            settings.FailedLoginAttempts = 0;
            settings.LockedUntil = null;
            user.LastLoginAt = DateTime.UtcNow;

            _unitOfWork.AuthRepository.UpdateUser(user);
            await _unitOfWork.SaveChangesAsync();

            var accessToken = _jwtTokenService.GenerateAccessToken(user);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();
            await _jwtTokenService.SaveRefreshTokenAsync(user.UserId, refreshToken, ipAddress.ToString());

            var authLog = new AuthAuditLog
            {
                UserId = user.UserId,
                Email = user.Email,
                ActionType = AuthActionType.LoginSuccess,
                IpAddress = ipAddress.ToString(),
                UserAgent = userAgent,
                ActionDate = DateTime.UtcNow
            };
            await _unitOfWork.AuthRepository.AddAuthLogAsync(authLog);
            await _unitOfWork.SaveChangesAsync();

            var response = new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                UserProfile = new UserProfileDTO
                {
                    UserId = user.UserId,
                    Email = user.Email,
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber ?? "",
                    KycStatus = user.KycStatus,
                    BiometricEnabled = user.BiometricEnabled,
                    TwoFactorEnabled = user.SecuritySettings?.TwoFactorEnabled ?? false,
                    LastLoginAt = user.LastLoginAt,
                    Preferences = new UserPreferencesDto
                    {
                        Language = user.UserPreferences?.Language ?? "en",
                        Currency = user.UserPreferences?.Currency ?? "USD",
                        Theme = user.UserPreferences?.Theme ?? Theme.Light,
                        NotificationsEnabled = user.UserPreferences?.NotificationsEnabled ?? true
                    }
                }
            };

            return Result<AuthResponse>.Success(response);
        }
        public async Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, IPAddress ipAddress)
        {
            var user = await _jwtTokenService.GetUserByRefreshTokenAsync(request.RefreshToken);
            if (user == null) return Result<AuthResponse>.Failure("Invalid refresh token.");

            var newAccessToken = _jwtTokenService.GenerateAccessToken(user);
            var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

            await _jwtTokenService.RevokeRefreshTokenAsync(request.RefreshToken, ipAddress.ToString(), newRefreshToken);
            await _jwtTokenService.SaveRefreshTokenAsync(user.UserId, newRefreshToken, ipAddress.ToString());

            var authResponse = new AuthResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                UserProfile = new UserProfileDTO
                {
                    UserId = user.UserId,
                    Email = user.Email,
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber ?? "",
                    KycStatus = user.KycStatus,
                    BiometricEnabled = user.BiometricEnabled,
                    TwoFactorEnabled = user.SecuritySettings?.TwoFactorEnabled ?? false,
                    LastLoginAt = user.LastLoginAt,
                    Preferences = new UserPreferencesDto
                    {
                        Language = user.UserPreferences?.Language ?? "en",
                        Currency = user.UserPreferences?.Currency ?? "USD",
                        Theme = user.UserPreferences?.Theme ?? Theme.Light,
                        NotificationsEnabled = user.UserPreferences?.NotificationsEnabled ?? true
                    }
                }
            };

            return Result<AuthResponse>.Success(authResponse);
        }
        public async Task<Result<bool>> SignOutAsync(string refreshToken, IPAddress ipAddress)
        {
            var strategy = _unitOfWork.DbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    var user = await _jwtTokenService.GetUserByRefreshTokenAsync(refreshToken);
                    if (user == null)
                        return Result<bool>.Failure("Invalid token.");

                    await _jwtTokenService.RevokeRefreshTokenAsync(refreshToken, ipAddress.ToString(), null);

                    var authLog = new AuthAuditLog
                    {
                        UserId = user.UserId,
                        Email = user.Email,
                        ActionType = AuthActionType.Logout,
                        IpAddress = ipAddress.ToString(),
                        UserAgent = string.Empty,
                        ActionDate = DateTime.UtcNow
                    };

                    await _unitOfWork.AuthRepository.AddAuthLogAsync(authLog);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    return Result<bool>.Success(true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result<bool>.Failure("An error occurred during sign-out.");
                }
            });
        }
        public async Task<Result<bool>> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            var strategy = _unitOfWork.DbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    var user = await _unitOfWork.AuthRepository.GetByEmailAsync(request.Email);
                    if (user == null || user.IsDeleted)
                        return Result<bool>.Failure("Account not found.");

                    var otp = await _otpService.GenerateOTPAsync(request.Email, request.PhoneNumber, OTPPurpose.PasswordReset);
                    await _otpService.SendOTPAsync(request.Email, request.PhoneNumber, otp);

                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    return Result<bool>.Success(true);
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogError(ex, "Error during forgot password OTP generation");
                    return Result<bool>.Failure("An error occurred while processing the request.");
                }
            });
        }
        public async Task<Result<string>> VerifyOTPAsync(VerifyOTPRequest request)
        {
            var strategy = _unitOfWork.DbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    var user = await _unitOfWork.AuthRepository.GetByEmailAsync(request.Email);
                    if (user == null || user.IsDeleted)
                        return Result<string>.Failure("Account not found.");

                    var isValid = await _otpService.VerifyOTPAsync(
                        request.Email,
                        request.PhoneNumber,
                        request.Code.ToString(),
                        request.Purpose
                    );

                    if (!isValid)
                        return Result<string>.Failure("Invalid or expired OTP.");

                    await _otpService.InvalidateOTPAsync(request.Email, null, request.Purpose);

                    var verificationToken = _jwtTokenService.GenerateVerificationToken(user);

                    await _unitOfWork.AuthRepository.AddAuthLogAsync(new AuthAuditLog
                    {
                        UserId = user.UserId,
                        Email = user.Email,
                        ActionType = AuthActionType.OTPVerification,
                        ActionDate = DateTime.UtcNow
                    });

                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    return Result<string>.Success(verificationToken);
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogError(ex, "Error during OTP verification");
                    return Result<string>.Failure("An error occurred during OTP verification.");
                }
            });
        }
        public async Task<Result<bool>> ResendOTPAsync(ResendOTPRequest request)
        {
            var user = await _unitOfWork.AuthRepository.GetByEmailAsync(request.Email);
            if (user == null || user.IsDeleted)
                return Result<bool>.Failure("Account not found.");

            await _otpService.InvalidateOTPAsync(request.Email, request.PhoneNumber, request.Purpose);
            var otp = await _otpService.GenerateOTPAsync(request.Email, request.PhoneNumber, request.Purpose);
            await _otpService.SendOTPAsync(request.Email, request.PhoneNumber, otp);

            return Result<bool>.Success(true);
        }
        public async Task<Result<bool>> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var strategy = _unitOfWork.DbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    var user = await _unitOfWork.AuthRepository.GetByEmailAsync(request.Email);
                    if (user == null)
                        return Result<bool>.Failure("Account not found");

                    var principal = _jwtTokenService.ValidateVerificationToken(request.VerificationToken);

                    if (principal == null)
                        return Result<bool>.Failure("Invalid or expired verification token");

                    foreach (var claim in principal.Claims)
                    {
                        _logger.LogInformation("JWT Claim: {Type} = {Value}", claim.Type, claim.Value);
                    }

                    var tokenEmail =
                        principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value ??
                        principal.FindFirst(ClaimTypes.Email)?.Value;
                    if (tokenEmail != request.Email)
                        return Result<bool>.Failure("Verification token does not match email.");

                    user.SecuritySettings ??= new UserSecuritySettings();
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                    user.SecuritySettings.LastPasswordChangeAt = DateTime.UtcNow;

                    await _jwtTokenService.InvalidateAllRefreshTokensAsync(user.UserId);
                    await _otpService.InvalidateOTPAsync(request.Email, user.PhoneNumber, OTPPurpose.PasswordReset);

                    await _unitOfWork.AuthRepository.AddAuthLogAsync(new AuthAuditLog
                    {
                        UserId = user.UserId,
                        Email = user.Email,
                        ActionType = AuthActionType.PasswordReset,
                        ActionDate = DateTime.UtcNow
                    });

                    _unitOfWork.AuthRepository.UpdateUser(user);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    return Result<bool>.Success(true);
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogError(ex, "Error during password reset");
                    return Result<bool>.Failure("An error occurred while resetting the password.");
                }
            });
        }
        public async Task<Result<bool>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
        {
            var strategy = _unitOfWork.DbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    var user = await _unitOfWork.AuthRepository.GetByIdAsync(userId);
                    if (user == null || user.IsDeleted)
                        return Result<bool>.Failure("User not found.");

                    if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                        return Result<bool>.Failure("Current password is incorrect.");

                    if (BCrypt.Net.BCrypt.Verify(request.NewPassword, user.PasswordHash))
                        return Result<bool>.Failure("New password must be different from current password.");

                    user.SecuritySettings ??= new UserSecuritySettings();
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                    user.SecuritySettings.LastPasswordChangeAt = DateTime.UtcNow;

                    await _jwtTokenService.InvalidateAllRefreshTokensAsync(userId);

                    await _unitOfWork.AuthRepository.AddAuthLogAsync(new AuthAuditLog
                    {
                        UserId = user.UserId,
                        Email = user.Email,
                        ActionType = AuthActionType.PasswordChange,
                        ActionDate = DateTime.UtcNow
                    });

                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    return Result<bool>.Success(true);
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogError(ex, "Error during password change");
                    return Result<bool>.Failure("An error occurred while changing the password.");
                }
            });
        }
        public async Task<Result<bool>> SetBiometricEnabledAsync(Guid userId, bool enabled)
        {
            var strategy = _unitOfWork.DbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    var user = await _unitOfWork.AuthRepository.GetByIdAsync(userId);
                    if (user == null || user.IsDeleted)
                        return Result<bool>.Failure("User not found.");

                    user.BiometricEnabled = enabled;

                    await _unitOfWork.AuthRepository.AddAuthLogAsync(new AuthAuditLog
                    {
                        UserId = user.UserId,
                        Email = user.Email,
                        ActionType = enabled ? AuthActionType.BiometricEnabled : AuthActionType.BiometricDisabled,
                        ActionDate = DateTime.UtcNow
                    });

                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    return Result<bool>.Success(true);
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogError(ex, "Error updating biometric status");
                    return Result<bool>.Failure("An error occurred while updating biometric status.");
                }
            });
        }
        public async Task<Result<UserProfileDTO>> GetProfileAsync(Guid userId)
        {
            var user = await _unitOfWork.AuthRepository.GetUserWithPreferencesAsync(userId);
            if (user == null)
                return Result<UserProfileDTO>.Failure("User not found");

            var profile = new UserProfileDTO
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber ?? "",
                BiometricEnabled = user.BiometricEnabled,
                TwoFactorEnabled = user.SecuritySettings?.TwoFactorEnabled ?? false,
                KycStatus = user.KycStatus,
                LastLoginAt = user.LastLoginAt,
                Preferences = new UserPreferencesDto
                {
                    Language = user.UserPreferences?.Language ?? "en",
                    Currency = user.UserPreferences?.Currency ?? "USD",
                    Theme = user.UserPreferences?.Theme ?? Theme.Light,
                    NotificationsEnabled = user.UserPreferences?.NotificationsEnabled ?? true
                }
            };

            return Result<UserProfileDTO>.Success(profile);
        }

        public async Task<Result<bool>> UpdateProfileAsync(Guid userId, UpdateUserProfileRequest request)
        {
            var user = await _unitOfWork.AuthRepository.GetUserWithPreferencesAsync(userId);
            if (user == null)
                return Result<bool>.Failure("User not found");

            user.FullName = request.FullName;
            user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber;

            if (user.UserPreferences == null)
            {
                user.UserPreferences = new UserPreferences
                {
                    UserId = user.UserId,
                    Language = request.Preferences.Language,
                    Currency = request.Preferences.Currency,
                    Theme = request.Preferences.Theme,
                    NotificationsEnabled = request.Preferences.NotificationsEnabled
                };
            }
            else
            {
                user.UserPreferences.Language = request.Preferences.Language;
                user.UserPreferences.Currency = request.Preferences.Currency;
                user.UserPreferences.Theme = request.Preferences.Theme;
                user.UserPreferences.NotificationsEnabled = request.Preferences.NotificationsEnabled;
            }

            await _unitOfWork.SaveChangesAsync();
            return Result<bool>.Success(true);
        }
    }
}
