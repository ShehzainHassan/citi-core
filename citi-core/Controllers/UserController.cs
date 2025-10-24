using citi_core.Interfaces;
using citi_core.Models;
using Microsoft.AspNetCore.Mvc;

namespace citi_core.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpPost("add-user")]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            var result = await _userService.AddUserAsync(user);

            if (!result.IsSuccess)
            {
                return BadRequest(new { error = result.ErrorMessage });
            }

            return Ok(result.Value);
        }
    }
}
