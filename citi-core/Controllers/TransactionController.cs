using citi_core.Common;
using citi_core.Dto;
using citi_core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace citi_core.Controllers
{
    [ApiController]
    [Route("api/v1/transactions")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly ILogger<TransactionController> _logger;
        private readonly IPdfService _pdfService;
        private readonly ICsvService _csvService;

        public TransactionController(ITransactionService transactionService, ILogger<TransactionController> logger, IPdfService pdfService, ICsvService csvService)
        {
            _transactionService = transactionService;
            _logger = logger;
            _pdfService = pdfService;
            _csvService = csvService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetTransactions([FromQuery] Guid? accountId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var traceId = HttpContext.GetCorrelationId();
            var userId = User.GetUserId();

            if (userId == null) return Unauthorized(ApiResponse<PaginatedResponse<TransactionDto>>.FailureResponse("User ID not found in token", null, traceId));

            _logger.LogInformation("GetTransactions requested | TraceId: {TraceId} | UserId: {UserId} | AccountId: {AccountId} | Page: {Page} | PageSize: {PageSize}",
                traceId, userId, accountId, page, pageSize);

            var result = await _transactionService.GetTransactionsAsync(userId.Value, accountId, page, pageSize);

            return Ok(ApiResponse<PaginatedResponse<TransactionDto>>.SuccessResponse(result, "Transactions retrieved", traceId));
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransactionById(Guid id)
        {
            var traceId = HttpContext.GetCorrelationId();
            var userId = User.GetUserId();

            if (userId == null)
                return Unauthorized(ApiResponse<TransactionDto>.FailureResponse("User ID not found in token", null, traceId));

            var transaction = await _transactionService.GetTransactionByIdAsync(id, userId.Value);

            return transaction == null ? NotFound(ApiResponse<TransactionDto>.FailureResponse("Transaction not found or access denied", null, traceId))
                : Ok(ApiResponse<TransactionDto>.SuccessResponse(transaction, "Transaction retrieved", traceId));
        }

        [Authorize]
        [HttpGet("report")]
        public async Task<IActionResult> GetTransactionReport([FromQuery] TransactionReportRequest request)
        {
            var traceId = HttpContext.GetCorrelationId();
            var userId = User.GetUserId();

            if (userId == null)
                return Unauthorized(ApiResponse<TransactionReportResponse>.FailureResponse("User ID not found in token", null, traceId));

            _logger.LogInformation("Transaction report requested | TraceId: {TraceId} | UserId: {UserId} | AccountId: {AccountId} | CardId: {CardId} | From: {StartDate} | To: {EndDate}",
                traceId, userId, request.AccountId, request.CardId, request.StartDate, request.EndDate);

            var report = await _transactionService.GetTransactionReportAsync(userId.Value, request);

            return Ok(ApiResponse<TransactionReportResponse>.SuccessResponse(report, "Transaction report generated", traceId));
        }

        [Authorize]
        [HttpPost("transfer")]
        public async Task<IActionResult> ProcessTransfer([FromBody] TransferRequest request)
        {
            var traceId = HttpContext.GetCorrelationId();
            var userId = User.GetUserId();

            if (userId == null)
                return Unauthorized(ApiResponse<Guid>.FailureResponse("User ID not found in token", default, traceId));

            _logger.LogInformation("Transfer requested | TraceId: {TraceId} | UserId: {UserId} | From: {FromAccountId} | To: {ToAccountNumber} | Amount: {Amount}",
                traceId, userId, request.FromAccountId, request.ToAccountNumber, request.Amount);

            var result = await _transactionService.ProcessTransferAsync(userId.Value, request);

            return result.IsSuccess
                ? Ok(ApiResponse<Guid>.SuccessResponse(result.Value, "Transfer successful", traceId))
                : BadRequest(ApiResponse<Guid>.FailureResponse(result.ErrorMessage!, default, traceId));
        }

        [Authorize]
        [HttpPost("withdrawal")]
        public async Task<IActionResult> ProcessWithdrawal([FromBody] WithdrawalRequest request)
        {
            var traceId = HttpContext.GetCorrelationId();
            var userId = User.GetUserId();

            if (userId == null)
                return Unauthorized(ApiResponse<Guid>.FailureResponse("User ID not found in token", default, traceId));

            _logger.LogInformation("Withdrawal requested | TraceId: {TraceId} | UserId: {UserId} | From: {FromAccountId} | Amount: {Amount}",
                traceId, userId, request.FromAccountId, request.Amount);

            var result = await _transactionService.ProcessWithdrawalAsync(userId.Value, request);

            return result.IsSuccess
                ? Ok(ApiResponse<Guid>.SuccessResponse(result.Value, "Withdrawal successful", traceId))
                : BadRequest(ApiResponse<Guid>.FailureResponse(result.ErrorMessage!, default, traceId));
        }

        [Authorize]
        [HttpPost("bill-payment")]
        public async Task<IActionResult> ProcessBillPayment([FromBody] BillPaymentRequest request)
        {
            var traceId = HttpContext.GetCorrelationId();
            var userId = User.GetUserId();

            if (userId == null)
                return Unauthorized(ApiResponse<Guid>.FailureResponse("User ID not found in token", default, traceId));

            _logger.LogInformation("Bill payment requested | TraceId: {TraceId} | UserId: {UserId} | From: {FromAccountId} | Biller: {BillerName} | Amount: {Amount}",
                traceId, userId, request.FromAccountId, request.BillerName, request.Amount);

            var result = await _transactionService.ProcessBillPaymentAsync(userId.Value, request);

            return result.IsSuccess
                ? Ok(ApiResponse<Guid>.SuccessResponse(result.Value, "Bill payment successful", traceId))
                : BadRequest(ApiResponse<Guid>.FailureResponse(result.ErrorMessage!, default, traceId));
        }

        [Authorize]
        [HttpPost("deposit")]
        public async Task<IActionResult> ProcessDeposit([FromBody] DepositRequest request)
        {
            var traceId = HttpContext.GetCorrelationId();
            var userId = User.GetUserId();

            if (userId == null)
                return Unauthorized(ApiResponse<Guid>.FailureResponse("User ID not found in token", default, traceId));

            _logger.LogInformation("Deposit requested | TraceId: {TraceId} | UserId: {UserId} | To: {ToAccountId} | Amount: {Amount} | Source: {Source}",
                traceId, userId, request.ToAccountId, request.Amount, request.Source);

            var result = await _transactionService.ProcessDepositAsync(userId.Value, request);

            return result.IsSuccess
                ? Ok(ApiResponse<Guid>.SuccessResponse(result.Value, "Deposit successful", traceId))
                : BadRequest(ApiResponse<Guid>.FailureResponse(result.ErrorMessage!, default, traceId));
        }

        [Authorize]
        [HttpPost("report/pdf")]
        public async Task<IActionResult> ExportTransactionsToPDF([FromBody] TransactionReportRequest request)
        {
            var traceId = HttpContext.GetCorrelationId();
            var userId = User.GetUserId();

            if (userId == null)
                return Unauthorized("User ID not found in token");

            _logger.LogInformation("Exporting transaction report to PDF | TraceId: {TraceId} | UserId: {UserId}", traceId, userId);

            var report = await _transactionService.GetTransactionReportAsync(userId.Value, request);

            var pdfBytes = _pdfService.GenerateTransactionReportPdf(report);

            return File(pdfBytes, "application/pdf", $"TransactionReport_{DateTime.UtcNow:yyyyMMdd}.pdf");
        }

        [Authorize]
        [HttpPost("report/csv")]
        public async Task<IActionResult> ExportTransactionsToCSV([FromBody] TransactionReportRequest request)
        {
            var traceId = HttpContext.GetCorrelationId();
            var userId = User.GetUserId();

            if (userId == null)
                return Unauthorized("User ID not found in token");

            _logger.LogInformation("Exporting transaction report to CSV | TraceId: {TraceId} | UserId: {UserId}", traceId, userId);

            var report = await _transactionService.GetTransactionReportAsync(userId.Value, request);

            var csv = _csvService.GenerateTransactionReportCsv(report);

            var bytes = Encoding.UTF8.GetBytes(csv);
            return File(bytes, "text/csv", $"TransactionReport_{DateTime.UtcNow:yyyyMMdd}.csv");
        }
    }
}