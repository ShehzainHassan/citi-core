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
            _logger.LogInformation("SignIn API Hit!");
            var ipAddress = HttpContext.Connection.RemoteIpAddress ?? IPAddress.None;
            var userAgent = Request.Headers["User-Agent"].ToString();

            var result = await _authService.SignInAsync(request, ipAddress, userAgent);
            if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });

            return Ok(result.Value);
        }
        
        /// <summary>
        /// User registration (sign-up)
        /// POST: /auth/signup
        /// </summary>
        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
        {
            _logger.LogInformation("SignUp API Hit!");
            var ipAddress = HttpContext.Connection.RemoteIpAddress ?? IPAddress.None;

            var userAgent = Request.Headers["User-Agent"].ToString();

            var result = await _authService.SignUpAsync(request, ipAddress, userAgent);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.ErrorMessage });

            return Ok(result.Value);
        }

        [Authorize]
        [HttpPost("signout")]
        public async Task<IActionResult> SignOut([FromBody] SignOutRequest request)
        {
            _logger.LogInformation("SignOut API Hit!");
            var ipAddress = HttpContext.Connection.RemoteIpAddress ?? IPAddress.None;

            var result = await _authService.SignOutAsync(request.RefreshToken, ipAddress);
            if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });

            return Ok(new { success = true });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            _logger.LogInformation("Refresh API Hit!");
            var ipAddress = HttpContext.Connection.RemoteIpAddress ?? IPAddress.None;

            var result = await _authService.RefreshTokenAsync(request, ipAddress);
            if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });

            return Ok(result.Value);
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            _logger.LogInformation("ForgotPassword API Hit!");

            var result = await _authService.ForgotPasswordAsync(request);
            if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });

            return Ok(new { success = true });
        }

        [HttpPost("verify-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyOTP([FromBody] VerifyOTPRequest request)
        {
            _logger.LogInformation("VerifyOTP API Hit!");

            var result = await _authService.VerifyOTPAsync(request);
            if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });

            return Ok(new { verificationToken = result.Value });
        }

        [HttpPost("resend-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> ResendOTP([FromBody] ResendOTPRequest request)
        {
            _logger.LogInformation("ResendOTP API Hit!");

            var result = await _authService.ResendOTPAsync(request);
            if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });

            return Ok(new { success = true });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            _logger.LogInformation("ResetPassword API Hit!");

            var result = await _authService.ResetPasswordAsync(request);
            if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });

            return Ok(new { success = true });
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            _logger.LogInformation("ChangePassword API Hit!");

            var userIdClaim = User.FindFirst("sub")
                              ?? User.FindFirst(ClaimTypes.NameIdentifier)
                              ?? User.FindFirst("userId");

            if (userIdClaim == null)
                return Unauthorized(new { message = "User ID not found in token" });

            var userId = Guid.Parse(userIdClaim.Value);

            var result = await _authService.ChangePasswordAsync(userId, request);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.ErrorMessage });

            return Ok(new { success = true });
        }

        [Authorize]
        [HttpPost("enable-biometric")]
        public async Task<IActionResult> EnableBiometric()
        {
            _logger.LogInformation("EnableBiometric API Hit!");

            var userIdClaim = User.FindFirst("sub")
                              ?? User.FindFirst(ClaimTypes.NameIdentifier)
                              ?? User.FindFirst("userId");

            if (userIdClaim == null)
                return Unauthorized(new { message = "User ID not found in token" });

            var userId = Guid.Parse(userIdClaim.Value);

            var result = await _authService.SetBiometricEnabledAsync(userId, true);
            if (!result.IsSuccess)
                return BadRequest(new { message = result.ErrorMessage });

            return Ok(new { success = true });
        }

        [Authorize]
        [HttpPost("disable-biometric")]
        public async Task<IActionResult> DisableBiometric()
        {
            _logger.LogInformation("DisableBiometric API Hit!");

            var userIdClaim = User.FindFirst("sub")
                              ?? User.FindFirst(ClaimTypes.NameIdentifier)
                              ?? User.FindFirst("userId");

            if (userIdClaim == null)
                return Unauthorized(new { message = "User ID not found in token" });

            var userId = Guid.Parse(userIdClaim.Value);

            var result = await _authService.SetBiometricEnabledAsync(userId, false);
            if (!result.IsSuccess)
                return BadRequest(new { message = result.ErrorMessage });

            return Ok(new { success = true });
        }
    }
}
