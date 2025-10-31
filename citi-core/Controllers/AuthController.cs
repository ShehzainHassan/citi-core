using citi_core.Common;
using citi_core.Common.citi_core.Common;
using citi_core.Dto;
using citi_core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace citi_core.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }
 
        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] SignInRequest request)
        {
            var correlationId = HttpContext.GetCorrelationId();
            var ipAddress = HttpContext.GetIpAddress();
            var userAgent = HttpContext.GetUserAgent();

            _logger.LogInformation("SignIn attempt | CorrelationId: {CorrelationId} | IP: {IPAddress} | UserAgent: {UserAgent}",
                correlationId, ipAddress, userAgent);

            var result = await _authService.SignInAsync(request, IPAddress.Parse(ipAddress), userAgent);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("SignIn failed | CorrelationId: {CorrelationId} | Reason: {ErrorMessage}",
                    correlationId, result.ErrorMessage);
                return BadRequest(new { message = result.ErrorMessage });
            }

            _logger.LogInformation("SignIn successful | CorrelationId: {CorrelationId} | UserId: {UserId}",
                correlationId, result.Value!.UserProfile.UserId);

            return Ok(result.Value);
        }

        /// <summary>
        /// User registration (sign-up)
        /// POST: /auth/signup
        /// </summary>
        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
        {
            var correlationId = HttpContext.GetCorrelationId();
            var ipAddress = HttpContext.Connection.RemoteIpAddress ?? IPAddress.None;
            var userAgent = HttpContext.GetUserAgent();

            _logger.LogInformation("SignUp attempt | CorrelationId: {CorrelationId} | IP: {IPAddress} | UserAgent: {UserAgent}",
                correlationId, ipAddress, userAgent);

            var result = await _authService.SignUpAsync(request, ipAddress, userAgent);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("SignUp failed | CorrelationId: {CorrelationId} | Reason: {ErrorMessage}",
                    correlationId, result.ErrorMessage);
                return BadRequest(new { message = result.ErrorMessage });
            }

            _logger.LogInformation("SignUp successful | CorrelationId: {CorrelationId} | UserId: {UserId}",
                correlationId, result.Value!.UserProfile.UserId);

            return Ok(result.Value);
        }

        [Authorize]
        [HttpPost("signout")]
        public async Task<IActionResult> SignOut([FromBody] SignOutRequest request)
        {
            var correlationId = HttpContext.GetCorrelationId();
            var ipAddress = HttpContext.GetIpAddress();

            _logger.LogInformation("SignOut attempt | CorrelationId: {CorrelationId} | IP: {IPAddress}",
                correlationId, ipAddress);

            var result = await _authService.SignOutAsync(request.RefreshToken, IPAddress.Parse(ipAddress));

            if (!result.IsSuccess)
            {
                _logger.LogWarning("SignOut failed | CorrelationId: {CorrelationId} | Reason: {ErrorMessage}",
                    correlationId, result.ErrorMessage);
                return BadRequest(new { message = result.ErrorMessage });
            }

            _logger.LogInformation("SignOut successful | CorrelationId: {CorrelationId}", correlationId);
            return Ok(new { success = true });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            var correlationId = HttpContext.GetCorrelationId();
            var ipAddress = HttpContext.GetIpAddress();

            _logger.LogInformation("Refresh token attempt | CorrelationId: {CorrelationId} | IP: {IPAddress}",
                correlationId, ipAddress);

            var result = await _authService.RefreshTokenAsync(request, IPAddress.Parse(ipAddress));

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Refresh token failed | CorrelationId: {CorrelationId} | Reason: {ErrorMessage}",
                    correlationId, result.ErrorMessage);
                return BadRequest(new { message = result.ErrorMessage });
            }

            _logger.LogInformation("Refresh token successful | CorrelationId: {CorrelationId}", correlationId);
            return Ok(result.Value);
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var correlationId = HttpContext.GetCorrelationId();
            _logger.LogInformation("ForgotPassword requested | CorrelationId: {CorrelationId}", correlationId);

            var result = await _authService.ForgotPasswordAsync(request);
            return result.IsSuccess ? Ok(new { success = true }) : BadRequest(new { message = result.ErrorMessage });
        }

        [HttpPost("verify-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyOTP([FromBody] VerifyOTPRequest request)
        {
            var correlationId = HttpContext.GetCorrelationId();
            _logger.LogInformation("VerifyOTP requested | CorrelationId: {CorrelationId}", correlationId);

            var result = await _authService.VerifyOTPAsync(request);
            return result.IsSuccess ? Ok(new { verificationToken = result.Value }) : BadRequest(new { message = result.ErrorMessage });
        }

        [HttpPost("resend-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> ResendOTP([FromBody] ResendOTPRequest request)
        {
            var correlationId = HttpContext.GetCorrelationId();
            _logger.LogInformation("ResendOTP requested | CorrelationId: {CorrelationId}", correlationId);

            var result = await _authService.ResendOTPAsync(request);
            return result.IsSuccess ? Ok(new { success = true }) : BadRequest(new { message = result.ErrorMessage });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var correlationId = HttpContext.GetCorrelationId();
            _logger.LogInformation("ResetPassword requested | CorrelationId: {CorrelationId}", correlationId);

            var result = await _authService.ResetPasswordAsync(request);
            return result.IsSuccess ? Ok(new { success = true }) : BadRequest(new { message = result.ErrorMessage });
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var correlationId = HttpContext.GetCorrelationId();
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized(new { message = "User ID not found in token" });

            _logger.LogInformation("ChangePassword requested | CorrelationId: {CorrelationId} | UserId: {UserId}",
                correlationId, userId);

            var result = await _authService.ChangePasswordAsync(userId.Value, request);
            return result.IsSuccess ? Ok(new { success = true }) : BadRequest(new { message = result.ErrorMessage });
        }

        [Authorize]
        [HttpPost("enable-biometric")]
        public async Task<IActionResult> EnableBiometric()
        {
            var correlationId = HttpContext.GetCorrelationId();
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized(new { message = "User ID not found in token" });

            _logger.LogInformation("EnableBiometric requested | CorrelationId: {CorrelationId} | UserId: {UserId}",
                correlationId, userId);

            var result = await _authService.SetBiometricEnabledAsync(userId.Value, true);
            return result.IsSuccess ? Ok(new { success = true }) : BadRequest(new { message = result.ErrorMessage });
        }

        [Authorize]
        [HttpPost("disable-biometric")]
        public async Task<IActionResult> DisableBiometric()
        {
            var correlationId = HttpContext.GetCorrelationId();
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized(new { message = "User ID not found in token" });

            _logger.LogInformation("DisableBiometric requested | CorrelationId: {CorrelationId} | UserId: {UserId}",
                correlationId, userId);

            var result = await _authService.SetBiometricEnabledAsync(userId.Value, false);
            return result.IsSuccess ? Ok(new { success = true }) : BadRequest(new { message = result.ErrorMessage });
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var correlationId = HttpContext.GetCorrelationId();
            var userId = User.GetUserId();

            if (userId == null)
            {
                _logger.LogWarning("GetProfile failed | CorrelationId: {CorrelationId} | Reason: Missing user ID claim", correlationId);
                return Unauthorized(new { message = "User ID not found in token" });
            }

            _logger.LogInformation("GetProfile requested | CorrelationId: {CorrelationId} | UserId: {UserId}", correlationId, userId);

            var profile = await _authService.GetProfileAsync(userId.Value);

            _logger.LogInformation("GetProfile successful | CorrelationId: {CorrelationId} | UserId: {UserId}", correlationId, userId);

            return Ok(profile);
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileRequest request)
        {
            var correlationId = HttpContext.GetCorrelationId();
            var userId = User.GetUserId();

            if (userId == null)
            {
                _logger.LogWarning("UpdateProfile failed | CorrelationId: {CorrelationId} | Reason: Missing user ID claim", correlationId);
                return Unauthorized(new { message = "User ID not found in token" });
            }

            _logger.LogInformation("UpdateProfile requested | CorrelationId: {CorrelationId} | UserId: {UserId}", correlationId, userId);

            var result = await _authService.UpdateProfileAsync(userId.Value, request);
            if (!result.IsSuccess)
            {
                _logger.LogWarning("UpdateProfile failed | CorrelationId: {CorrelationId} | UserId: {UserId} | Reason: {ErrorMessage}",
                    correlationId, userId, result.ErrorMessage);
                return BadRequest(new { message = result.ErrorMessage });
            }

            var updatedProfile = await _authService.GetProfileAsync(userId.Value);

            _logger.LogInformation("UpdateProfile successful | CorrelationId: {CorrelationId} | UserId: {UserId}", correlationId, userId);

            return Ok(updatedProfile);
        }

    }
}
