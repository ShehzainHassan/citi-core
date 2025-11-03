using citi_core.Common;
using citi_core.Dto;
using citi_core.Interfaces;
using citi_core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace citi_core.Controllers
{
    [ApiController]
    [Route("api/v1/beneficiaries")]
    public class BeneficiaryController : ControllerBase
    {
        private readonly IBeneficiaryService _service;
        private readonly ILogger<BeneficiaryController> _logger;

        public BeneficiaryController(IBeneficiaryService service, ILogger<BeneficiaryController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetBeneficiaries()
        {
            var traceId = HttpContext.GetCorrelationId();
            var userId = User.GetUserId();

            if (userId == null)
            {
                _logger.LogWarning("GetBeneficiaries failed | TraceId: {TraceId} | Reason: Missing user ID claim", traceId);
                return Unauthorized(ApiResponse<List<Beneficiary>>.FailureResponse("User ID not found in token", null, traceId));
            }

            _logger.LogInformation("GetBeneficiaries requested | TraceId: {TraceId} | UserId: {UserId}", traceId, userId);

            var beneficiaries = await _service.GetBeneficiariesAsync(userId.Value);

            _logger.LogInformation("GetBeneficiaries successful | TraceId: {TraceId} | UserId: {UserId} | Count: {Count}", traceId, userId, beneficiaries.Count);

            return Ok(ApiResponse<List<Beneficiary>>.SuccessResponse(beneficiaries, "Beneficiaries retrieved", traceId));
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBeneficiaryById(Guid beneficiaryId)
        {
            var traceId = HttpContext.GetCorrelationId();
            var userId = User.GetUserId();

            if (userId == null)
            {
                _logger.LogWarning("GetBeneficiaryById failed | TraceId: {TraceId} | Reason: Missing user ID claim", traceId);
                return Unauthorized(ApiResponse<Beneficiary>.FailureResponse("User ID not found in token", null, traceId));
            }

            _logger.LogInformation("GetBeneficiaryById requested | TraceId: {TraceId} | UserId: {UserId} | BeneficiaryId: {BeneficiaryId}", traceId, userId, beneficiaryId);

            var beneficiary = await _service.GetBeneficiaryByIdAsync(beneficiaryId, userId.Value);

            if (beneficiary == null)
            {
                _logger.LogWarning("GetBeneficiaryById failed | TraceId: {TraceId} | UserId: {UserId} | BeneficiaryId: {BeneficiaryId} | Reason: Not found or unauthorized", traceId, userId, beneficiaryId);
                return NotFound(ApiResponse<Beneficiary>.FailureResponse("Beneficiary not found or access denied", null, traceId));
            }

            _logger.LogInformation("GetBeneficiaryById successful | TraceId: {TraceId} | UserId: {UserId} | BeneficiaryId: {BeneficiaryId}", traceId, userId, beneficiaryId);

            return Ok(ApiResponse<Beneficiary>.SuccessResponse(beneficiary, "Beneficiary details retrieved", traceId));
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddBeneficiary([FromBody] AddBeneficiaryRequest request)
        {
            var traceId = HttpContext.GetCorrelationId();
            var userId = User.GetUserId();

            if (userId == null)
            {
                _logger.LogWarning("AddBeneficiary failed | TraceId: {TraceId} | Reason: Missing user ID claim", traceId);
                return Unauthorized(ApiResponse<Guid>.FailureResponse("User ID not found in token", null, traceId));
            }

            _logger.LogInformation("AddBeneficiary requested | TraceId: {TraceId} | UserId: {UserId} | AccountNumber: {AccountNumber}", traceId, userId, request.AccountNumber);

            var result = await _service.AddBeneficiaryAsync(userId.Value, request);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("AddBeneficiary failed | TraceId: {TraceId} | UserId: {UserId} | Reason: {Error}", traceId, userId, result.ErrorMessage);
                return BadRequest(ApiResponse<Guid>.FailureResponse(result.ErrorMessage!, null, traceId));
            }

            _logger.LogInformation("AddBeneficiary successful | TraceId: {TraceId} | UserId: {UserId} | BeneficiaryId: {BeneficiaryId}", traceId, userId, result.Value);

            return Ok(ApiResponse<Guid>.SuccessResponse(result.Value, "Beneficiary added successfully", traceId));
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBeneficiary(Guid id, [FromBody] UpdateBeneficiaryRequest request)
        {
            var traceId = HttpContext.GetCorrelationId();
            var userId = User.GetUserId();

            if (userId == null)
            {
                _logger.LogWarning("UpdateBeneficiary failed | TraceId: {TraceId} | Reason: Missing user ID claim", traceId);
                return Unauthorized(ApiResponse<bool>.FailureResponse("User ID not found in token", null, traceId));
            }

            _logger.LogInformation("UpdateBeneficiary requested | TraceId: {TraceId} | UserId: {UserId} | BeneficiaryId: {BeneficiaryId}", traceId, userId, id);

            var result = await _service.UpdateBeneficiaryAsync(id, userId.Value, request);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("UpdateBeneficiary failed | TraceId: {TraceId} | UserId: {UserId} | BeneficiaryId: {BeneficiaryId} | Reason: {Error}", traceId, userId, id, result.ErrorMessage);
                return NotFound(ApiResponse<bool>.FailureResponse(result.ErrorMessage!, null, traceId));
            }

            _logger.LogInformation("UpdateBeneficiary successful | TraceId: {TraceId} | UserId: {UserId} | BeneficiaryId: {BeneficiaryId}", traceId, userId, id);

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Beneficiary updated successfully", traceId));
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBeneficiary(Guid id)
        {
            var traceId = HttpContext.GetCorrelationId();
            var userId = User.GetUserId();

            if (userId == null)
            {
                _logger.LogWarning("DeleteBeneficiary failed | TraceId: {TraceId} | Reason: Missing user ID claim", traceId);
                return Unauthorized(ApiResponse<bool>.FailureResponse("User ID not found in token", null, traceId));
            }

            _logger.LogInformation("DeleteBeneficiary requested | TraceId: {TraceId} | UserId: {UserId} | BeneficiaryId: {BeneficiaryId}", traceId, userId, id);

            var result = await _service.DeleteBeneficiaryAsync(id, userId.Value);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("DeleteBeneficiary failed | TraceId: {TraceId} | UserId: {UserId} | BeneficiaryId: {BeneficiaryId} | Reason: {Error}", traceId, userId, id, result.ErrorMessage);
                return NotFound(ApiResponse<bool>.FailureResponse(result.ErrorMessage!, null, traceId));
            }

            _logger.LogInformation("DeleteBeneficiary successful | TraceId: {TraceId} | UserId: {UserId} | BeneficiaryId: {BeneficiaryId}", traceId, userId, id);

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Beneficiary deleted successfully", traceId));
        }

        [Authorize]
        [HttpPost("{id}/favorite")]
        public async Task<IActionResult> SetFavorite(Guid id, [FromQuery] bool isFavorite)
        {
            var traceId = HttpContext.GetCorrelationId();
            var userId = User.GetUserId();

            if (userId == null)
            {
                _logger.LogWarning("SetFavorite failed | TraceId: {TraceId} | Reason: Missing user ID claim", traceId);
                return Unauthorized(ApiResponse<bool>.FailureResponse("User ID not found in token", null, traceId));
            }

            _logger.LogInformation("SetFavorite requested | TraceId: {TraceId} | UserId: {UserId} | BeneficiaryId: {BeneficiaryId} | IsFavorite: {IsFavorite}", traceId, userId, id, isFavorite);

            var result = await _service.SetFavoriteAsync(id, userId.Value, isFavorite);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("SetFavorite failed | TraceId: {TraceId} | UserId: {UserId} | BeneficiaryId: {BeneficiaryId} | Reason: {Error}", traceId, userId, id, result.ErrorMessage);
                return NotFound(ApiResponse<bool>.FailureResponse(result.ErrorMessage!, null, traceId));
            }

            _logger.LogInformation("SetFavorite successful | TraceId: {TraceId} | UserId: {UserId} | BeneficiaryId: {BeneficiaryId} | IsFavorite: {IsFavorite}", traceId, userId, id, isFavorite);

            return Ok(ApiResponse<bool>.SuccessResponse(true, isFavorite ? "Marked as favorite" : "Unmarked as favorite", traceId));
        }
    }
}
