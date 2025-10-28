using citi_core.Common.citi_core.Common;
using citi_core.Dto;
using citi_core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
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
    }
}
