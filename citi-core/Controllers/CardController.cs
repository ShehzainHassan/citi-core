using citi_core.Common;
using citi_core.Dto;
using citi_core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace citi_core.Controllers
{
    [ApiController]
    [Route("cards")]
    public class CardController : ControllerBase
    {
        private readonly ICardService _cardService;
        private readonly ILogger<CardController> _logger;

        public CardController(ICardService cardService, ILogger<CardController> logger)
        {
            _cardService = cardService;
            _logger = logger;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetUserCards()
        {
            var traceId = HttpContext.GetCorrelationId();
            var userId = User.GetUserId();

            if (userId == null)
            {
                _logger.LogWarning("GetUserCards failed | TraceId: {TraceId} | Reason: Missing user ID claim", traceId);
                return Unauthorized(ApiResponse<List<CardDto>>.FailureResponse("User ID not found in token", null, traceId));
            }

            _logger.LogInformation("GetUserCards requested | TraceId: {TraceId} | UserId: {UserId}", traceId, userId);

            var cards = await _cardService.GetUserCardsAsync(userId.Value);

            _logger.LogInformation("GetUserCards successful | TraceId: {TraceId} | UserId: {UserId} | Count: {Count}", traceId, userId, cards.Count);

            return Ok(ApiResponse<List<CardDto>>.SuccessResponse(cards, "Cards retrieved", traceId));
        }

        [Authorize]
        [HttpGet("{cardId}")]
        public async Task<IActionResult> GetCardDetails(Guid cardId)
        {
            var traceId = HttpContext.GetCorrelationId();
            var userId = User.GetUserId();

            if (userId == null)
            {
                _logger.LogWarning("GetCardDetails failed | TraceId: {TraceId} | Reason: Missing user ID claim", traceId);
                return Unauthorized(ApiResponse<CardDetailsDto>.FailureResponse("User ID not found in token", null, traceId));
            }

            _logger.LogInformation("GetCardDetails requested | TraceId: {TraceId} | UserId: {UserId} | CardId: {CardId}", traceId, userId, cardId);

            var card = await _cardService.GetCardDetailsAsync(cardId, userId.Value);

            if (card == null)
            {
                _logger.LogWarning("GetCardDetails failed | TraceId: {TraceId} | UserId: {UserId} | CardId: {CardId} | Reason: Not found or unauthorized", traceId, userId, cardId);
                return NotFound(ApiResponse<CardDetailsDto>.FailureResponse("Card not found or access denied", null, traceId));
            }

            _logger.LogInformation("GetCardDetails successful | TraceId: {TraceId} | UserId: {UserId} | CardId: {CardId}", traceId, userId, cardId);

            return Ok(ApiResponse<CardDetailsDto>.SuccessResponse(card, "Card details retrieved", traceId));
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddCardRequest([FromBody] AddCardRequest request)
        {
            var traceId = HttpContext.GetCorrelationId();
            var userId = User.GetUserId();

            if (userId == null)
            {
                _logger.LogWarning("AddCard failed | TraceId: {TraceId} | Reason: Missing user ID claim", traceId);
                return Unauthorized(ApiResponse<object>.FailureResponse("User ID not found in token", null, traceId));
            }

            _logger.LogInformation("AddCard requested | TraceId: {TraceId} | UserId: {UserId} | AccountId: {AccountId} | CardType: {CardType} | DesiredCreditLimit: {CreditLimit}",
                traceId, userId, request.AccountId, request.CardType, request.DesiredCreditLimit);

            var result = await _cardService.AddCardRequestAsync(userId.Value, request);

            if (!result.IsSuccess || result.Value == null)
            {
                _logger.LogWarning("AddCard failed | TraceId: {TraceId} | UserId: {UserId} | AccountId: {AccountId} | Reason: {Reason}",
                    traceId, userId, request.AccountId, result.ErrorMessage ?? "Unknown error");
                return BadRequest(ApiResponse<object>.FailureResponse(result.ErrorMessage ?? "Add card failed", null, traceId));
            }

            _logger.LogInformation("AddCard successful | TraceId: {TraceId} | UserId: {UserId} | AccountId: {AccountId} | CardId: {CardId}",
                traceId, userId, request.AccountId, result.Value.CardId);

            return Ok(ApiResponse<AddCardRequestDto>.SuccessResponse(result.Value, "Add card initiated", traceId));
        }

        [Authorize]
        [HttpPut("{cardId}")]
        public async Task<IActionResult> UpdateCard(Guid cardId, [FromBody] UpdateCardRequest request)
        {
            var traceId = HttpContext.GetCorrelationId();
            var userId = User.GetUserId();

            if (userId == null)
            {
                _logger.LogWarning("UpdateCard failed | TraceId: {TraceId} | Reason: Missing user ID", traceId);
                return Unauthorized(ApiResponse<bool>.FailureResponse("User ID not found", null, traceId));
            }

            _logger.LogInformation("UpdateCard requested | TraceId: {TraceId} | UserId: {UserId} | CardId: {CardId} | AccountId: {AccountId}",
                traceId, userId, cardId, request.AccountId);

            var result = await _cardService.UpdateCardAsync(cardId, userId.Value, request);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("UpdateCard failed | TraceId: {TraceId} | CardId: {CardId} | Reason: {Reason}",
                    traceId, cardId, result.ErrorMessage ?? "Unknown error");

                return NotFound(ApiResponse<bool>.FailureResponse(result.ErrorMessage ?? "Card update failed", null, traceId));
            }

            _logger.LogInformation("UpdateCard successful | TraceId: {TraceId} | CardId: {CardId}", traceId, cardId);
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Card updated", traceId));
        }

        [Authorize]
        [HttpPatch("{cardId}/status")]
        public async Task<IActionResult> UpdateCardStatus(Guid cardId, [FromBody] CardStatusUpdateRequest request)
        {
            var traceId = HttpContext.GetCorrelationId();
            var userId = User.GetUserId();

            if (userId == null)
            {
                _logger.LogWarning("UpdateCardStatus failed | TraceId: {TraceId} | Reason: Missing user ID", traceId);
                return Unauthorized(ApiResponse<bool>.FailureResponse("User ID not found", null, traceId));
            }

            _logger.LogInformation("UpdateCardStatus requested | TraceId: {TraceId} | UserId: {UserId} | CardId: {CardId} | Status: {Status} | Reason: {Reason}",
                traceId, userId, cardId, request.Status, request.Reason);

            var result = await _cardService.UpdateCardStatusAsync(cardId, userId.Value, request);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("UpdateCardStatus failed | TraceId: {TraceId} | CardId: {CardId} | Reason: {Reason}",
                    traceId, cardId, result.ErrorMessage ?? "Unknown error");

                return NotFound(ApiResponse<bool>.FailureResponse(result.ErrorMessage ?? "Card status update failed", null, traceId));
            }

            _logger.LogInformation("UpdateCardStatus successful | TraceId: {TraceId} | CardId: {CardId} | NewStatus: {Status}", traceId, cardId, request.Status);
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Card status updated", traceId));
        }

        [Authorize]
        [HttpDelete("{cardId}")]
        public async Task<IActionResult> DeleteCard(Guid cardId)
        {
            var traceId = HttpContext.GetCorrelationId();
            var userId = User.GetUserId();

            if (userId == null)
            {
                _logger.LogWarning("DeleteCard failed | TraceId: {TraceId} | Reason: Missing user ID", traceId);
                return Unauthorized(ApiResponse<bool>.FailureResponse("User ID not found", null, traceId));
            }

            _logger.LogInformation("DeleteCard requested | TraceId: {TraceId} | UserId: {UserId} | CardId: {CardId}", traceId, userId, cardId);

            var result = await _cardService.DeleteCardAsync(cardId, userId.Value);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("DeleteCard failed | TraceId: {TraceId} | CardId: {CardId} | Reason: {Reason}", traceId, cardId, result.ErrorMessage ?? "Unknown error");
                return NotFound(ApiResponse<bool>.FailureResponse(result.ErrorMessage ?? "Card deletion failed", null, traceId));
            }

            _logger.LogInformation("DeleteCard successful | TraceId: {TraceId} | CardId: {CardId}", traceId, cardId);
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Card deleted", traceId));
        }
        [Authorize]
        [HttpGet("{cardId}/limits")]
        public async Task<IActionResult> GetCardLimits(Guid cardId)
        {
            var traceId = HttpContext.GetCorrelationId();
            var userId = User.GetUserId();
            if (userId == null)
            {
                _logger.LogWarning("GetCardLimits failed | TraceId: {TraceId} | Reason: Missing user ID", traceId);
                return Unauthorized(ApiResponse<CardLimitsResponseDto>.FailureResponse("User ID not found", null, traceId));
            }

            var limits = await _cardService.GetCardLimitsAsync(cardId, userId.Value);
            if (limits == null)
            {
                _logger.LogWarning("GetCardLimits failed | TraceId: {TraceId} | CardId: {CardId} | Reason: Not found", traceId, cardId);
                return NotFound(ApiResponse<CardLimitsResponseDto>.FailureResponse("Card not found", null, traceId));
            }

            return Ok(ApiResponse<CardLimitsResponseDto>.SuccessResponse(limits, "Card limits retrieved", traceId));
        }

        [Authorize]
        [HttpPut("{cardId}/limits")]
        public async Task<IActionResult> UpdateCardLimits(Guid cardId, [FromBody] UpdateCardLimitsRequest request)
        {
            var traceId = HttpContext.GetCorrelationId();
            var userId = User.GetUserId();

            if (userId == null)
            {
                _logger.LogWarning("UpdateCardLimits failed | TraceId: {TraceId} | Reason: Missing user ID", traceId);
                return Unauthorized(ApiResponse<bool>.FailureResponse("User ID not found", null, traceId));
            }

            _logger.LogInformation("UpdateCardLimits requested | TraceId: {TraceId} | UserId: {UserId} | CardId: {CardId} | DailyLimit: {DailyLimit} | MonthlyLimit: {MonthlyLimit}",
                traceId, userId, cardId, request.DailyLimit, request.MonthlyLimit);

            var result = await _cardService.UpdateCardLimitsAsync(cardId, userId.Value, request);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("UpdateCardLimits failed | TraceId: {TraceId} | CardId: {CardId} | Reason: {Reason}",
                    traceId, cardId, result.ErrorMessage ?? "Unknown error");

                return BadRequest(ApiResponse<bool>.FailureResponse(result.ErrorMessage ?? "Card limits update failed", null, traceId));
            }

            _logger.LogInformation("UpdateCardLimits successful | TraceId: {TraceId} | CardId: {CardId}", traceId, cardId);
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Card limits updated", traceId));
        }
    }
}