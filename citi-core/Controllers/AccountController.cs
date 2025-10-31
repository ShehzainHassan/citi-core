using citi_core.Common;
using citi_core.Dto;
using citi_core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace citi_core.Controllers
{
    [ApiController]
    [Route("accounts")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAccountService accountService, ILogger<AccountController> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAccounts()
        {
            var traceId = HttpContext.GetCorrelationId();
            var userId = User.GetUserId();

            if (userId == null)
            {
                _logger.LogWarning("GetAccounts failed | TraceId: {TraceId} | Reason: Missing user ID claim", traceId);
                return Unauthorized(ApiResponse<List<AccountDto>>.FailureResponse("User ID not found in token", null, traceId));
            }

            _logger.LogInformation("GetAccounts requested | TraceId: {TraceId} | UserId: {UserId}", traceId, userId);

            var accounts = await _accountService.GetUserAccountsAsync(userId.Value);

            _logger.LogInformation("GetAccounts successful | TraceId: {TraceId} | UserId: {UserId} | Count: {Count}", traceId, userId, accounts.Count);

            return Ok(ApiResponse<List<AccountDto>>.SuccessResponse(accounts, "Accounts retrieved", traceId));
        }

        [Authorize]
        [HttpGet("{accountId}")]
        public async Task<IActionResult> GetAccountDetails(Guid accountId)
        {
            var traceId = HttpContext.GetCorrelationId();
            var userId = User.GetUserId();

            if (userId == null)
                return Unauthorized(ApiResponse<AccountDetailsDto>.FailureResponse("User ID not found in token", null, traceId));

            var details = await _accountService.GetAccountDetailsAsync(accountId, userId.Value);

            return details == null
                ? NotFound(ApiResponse<AccountDetailsDto>.FailureResponse("Account not found or access denied", null, traceId))
                : Ok(ApiResponse<AccountDetailsDto>.SuccessResponse(details, "Account details retrieved", traceId));
        }

        [Authorize]
        [HttpGet("{accountId}/balance")]
        public async Task<IActionResult> GetAccountBalance(Guid accountId)
        {
            var traceId = HttpContext.GetCorrelationId();
            var userId = User.GetUserId();

            if (userId == null)
            {
                _logger.LogWarning("GetAccountBalance failed | TraceId: {TraceId} | Reason: Missing user ID claim", traceId);
                return Unauthorized(ApiResponse<AccountBalanceDto>.FailureResponse("User ID not found in token", null, traceId));
            }

            _logger.LogInformation("GetAccountBalance requested | TraceId: {TraceId} | UserId: {UserId} | AccountId: {AccountId}", traceId, userId, accountId);

            var balance = await _accountService.GetAccountBalanceAsync(accountId, userId.Value);

            if (balance == null)
            {
                _logger.LogWarning("GetAccountBalance failed | TraceId: {TraceId} | UserId: {UserId} | AccountId: {AccountId} | Reason: Not found or unauthorized", traceId, userId, accountId);
                return NotFound(ApiResponse<AccountBalanceDto>.FailureResponse("Account not found or access denied", null, traceId));
            }

            _logger.LogInformation("GetAccountBalance successful | TraceId: {TraceId} | UserId: {UserId} | AccountId: {AccountId}", traceId, userId, accountId);

            return Ok(ApiResponse<AccountBalanceDto>.SuccessResponse(balance, "Balance retrieved", traceId));
        }

        [Authorize]
        [HttpDelete("{accountId}")]
        public async Task<IActionResult> CloseAccount(Guid accountId)
        {
            var traceId = HttpContext.GetCorrelationId();
            var userId = User.GetUserId();

            if (userId == null)
            {
                _logger.LogWarning("CloseAccount failed | TraceId: {TraceId} | Reason: Missing user ID claim", traceId);
                return Unauthorized(ApiResponse<bool>.FailureResponse("User ID not found in token", null, traceId));
            }

            _logger.LogInformation("CloseAccount requested | TraceId: {TraceId} | UserId: {UserId} | AccountId: {AccountId}", traceId, userId, accountId);

            var result = await _accountService.CloseAccountAsync(accountId, userId.Value);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("CloseAccount failed | TraceId: {TraceId} | UserId: {UserId} | AccountId: {AccountId} | Reason: {ErrorMessage}", traceId, userId, accountId, result.ErrorMessage);
                return BadRequest(ApiResponse<bool>.FailureResponse(result.ErrorMessage!, null, traceId));
            }

            _logger.LogInformation("CloseAccount successful | TraceId: {TraceId} | UserId: {UserId} | AccountId: {AccountId}", traceId, userId, accountId);

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Account closed successfully", traceId));
        }

        [Authorize]
        [HttpGet("{accountId}/statement")]
        public async Task<IActionResult> GetAccountStatement(Guid accountId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var traceId = HttpContext.GetCorrelationId();
            var userId = User.GetUserId();

            if (userId == null)
            {
                _logger.LogWarning("GetAccountStatement failed | TraceId: {TraceId} | Reason: Missing user ID claim", traceId);
                return Unauthorized(ApiResponse<object>.FailureResponse("User ID not found in token", null, traceId));
            }

            _logger.LogInformation("GetAccountStatement requested | TraceId: {TraceId} | UserId: {UserId} | AccountId: {AccountId} | Page: {Page} | PageSize: {PageSize}",
                traceId, userId, accountId, page, pageSize);

            var statement = await _accountService.GetAccountStatementAsync(accountId, userId.Value, startDate, endDate, page, pageSize);

            _logger.LogInformation("GetAccountStatement successful | TraceId: {TraceId} | UserId: {UserId} | AccountId: {AccountId} | Returned: {Count}",
                traceId, userId, accountId, statement.Items.Count);

            return Ok(ApiResponse<PaginatedResponse<TransactionDto>>.SuccessResponse(statement, "Statement retrieved", traceId));
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request)
        {
            var traceId = HttpContext.GetCorrelationId();
            var userId = User.GetUserId();

            if (userId == null)
            {
                _logger.LogWarning("CreateAccount failed | TraceId: {TraceId} | Reason: Missing user ID claim", traceId);
                return Unauthorized(ApiResponse<Guid>.FailureResponse("User ID not found in token", default, traceId));
            }

            _logger.LogInformation("CreateAccount requested | TraceId: {TraceId} | UserId: {UserId}", traceId, userId);

            var result = await _accountService.AddAccountAsync(userId.Value, request);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("CreateAccount failed | TraceId: {TraceId} | UserId: {UserId} | Reason: {ErrorMessage}", traceId, userId, result.ErrorMessage);
                return BadRequest(ApiResponse<Guid>.FailureResponse(result.ErrorMessage!, default, traceId));
            }

            _logger.LogInformation("CreateAccount successful | TraceId: {TraceId} | UserId: {UserId} | AccountId: {AccountId}", traceId, userId, result.Value);

            return Ok(ApiResponse<Guid>.SuccessResponse(result.Value, "Account created successfully", traceId));
        }
    }
}