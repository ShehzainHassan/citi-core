using citi_core.Common.citi_core.Common;
using citi_core.Data;
using citi_core.Dto;
using citi_core.Enums;
using citi_core.Interfaces;
using citi_core.Models;
using citi_core.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace citi_core.Services
{
    public class CardService : ICardService
    {
        private readonly ICardRepository _cardRepo;
        private readonly IDistributedCache _cache;
        private readonly IEncryptionService _encryption;
        private readonly ILogger<CardService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        public CardService(ICardRepository cardRepo, IDistributedCache cache, IEncryptionService encryption, ILogger<CardService> logger, IUnitOfWork unitOfWork)
        {
            _cardRepo = cardRepo;
            _cache = cache;
            _encryption = encryption;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }
        public async Task<List<CardDto>> GetUserCardsAsync(Guid userId)
        {
            var cacheKey = $"user_cards_{userId}";
            var cached = await _cache.GetStringAsync(cacheKey);
            if (cached != null)
                return JsonSerializer.Deserialize<List<CardDto>>(cached)!;

            var cards = await _cardRepo.GetUserCardsAsync(userId);

            var dtos = cards.Select(c => new CardDto
            {
                CardId = c.CardId,
                MaskedCardNumber = CardMasking.Mask(c.Last4Digits),
                CardName = c.CardName,
                CardType = c.CardType,
                CardBrand = c.CardBrand,
                AvailableCredit = c.CardType == CardType.Credit ? c.AvailableCredit : null,
                AccountBalance = c.CardType == CardType.Debit ? c.Account.Balance : null
            }).ToList();

            var json = JsonSerializer.Serialize(dtos);
            await _cache.SetStringAsync(cacheKey, json, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });

            return dtos;
        }
        public async Task<CardDetailsDto?> GetCardDetailsAsync(Guid cardId, Guid userId)
        {
            var card = await _cardRepo.GetCardByIdAsync(cardId, userId);
            if (card == null) return null;

            var isExpired = DateTime.TryParseExact("01/" + card.ExpiryDate, "dd/MM/yy", null, System.Globalization.DateTimeStyles.None, out var expiryDate)
                && expiryDate < DateTime.UtcNow;

            var today = DateTime.UtcNow.Date;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            var usedToday = card.Transactions
                .Where(t => t.TransactionDate.Date == today && t.Status == TransactionStatus.Completed)
                .Sum(t => t.Amount);

            var usedThisMonth = card.Transactions
                .Where(t => t.TransactionDate >= startOfMonth && t.TransactionDate <= today && t.Status == TransactionStatus.Completed)
                .Sum(t => t.Amount);

            return new CardDetailsDto
            {
                CardId = card.CardId,
                MaskedCardNumber = CardMasking.Mask(card.Last4Digits),
                CardHolderName = card.CardHolderName,
                CardName = card.CardName,
                CardType = card.CardType,
                CardBrand = card.CardBrand,
                ValidFrom = card.ValidFrom,
                ExpiryDate = card.ExpiryDate,
                IsExpired = isExpired,
                AvailableCredit = card.CardType == CardType.Credit ? card.AvailableCredit : null,
                AccountBalance = card.CardType == CardType.Debit ? card.Account.Balance : null,
                DailyLimit = card.DailyLimit,
                MonthlyLimit = card.MonthlyLimit,
                UsedToday = usedToday,
                UsedThisMonth = usedThisMonth,
                LinkedAccount = new AccountDto
                {
                    AccountId = card.Account.AccountId,
                    MaskedAccountNumber = AccountMaskingHelper.MaskAccountNumber(card.Account.AccountNumber),
                    Balance = card.Account.Balance,
                    Branch = card.Account.Branch,
                    Status = card.Account.Status
                }
            };
        }
        public async Task<Result<AddCardRequestDto>> AddCardRequestAsync(Guid userId, AddCardRequest request)
        {
            var strategy = _unitOfWork.DbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    var accountExists = await _cardRepo.AccountExistsAsync(request.AccountId, userId);
                    if (!accountExists)
                    {
                        _logger.LogWarning("Card request failed | Reason: Account not found or access denied | AccountId: {AccountId} | UserId: {UserId}", request.AccountId, userId);
                        return Result<AddCardRequestDto>.Failure("Account not found or access denied.");
                    }

                    var cardRequest = new CardRequest
                    {
                        CardRequestId = Guid.NewGuid(),
                        UserId = userId,
                        AccountId = request.AccountId,
                        CardType = request.CardType,
                        CardHolderName = request.CardHolderName,
                        CardName = request.CardName,
                        DesiredCreditLimit = request.DesiredCreditLimit,
                        Status = CardStatus.Pending,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _cardRepo.AddCardRequestAsync(cardRequest);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    _logger.LogInformation("Card request submitted | CardRequestId: {CardRequestId} | UserId: {UserId} | AccountId: {AccountId}", cardRequest.CardRequestId, userId, request.AccountId);

                    var response = new AddCardRequestDto
                    {
                        CardId = cardRequest.CardRequestId,
                        Status = cardRequest.Status.ToString()
                    };

                    return Result<AddCardRequestDto>.Success(response);
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogError(ex, "Card request failed | UserId: {UserId} | AccountId: {AccountId}", userId, request.AccountId);
                    return Result<AddCardRequestDto>.Failure("An error occurred during card request submission.");
                }
            });
        }
        public async Task<Result<bool>> UpdateCardAsync(Guid cardId, Guid userId, UpdateCardRequest dto)
        {
            var strategy = _unitOfWork.DbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    var card = await _cardRepo.GetCardByIdAsync(cardId, userId);
                    if (card == null)
                    {
                        _logger.LogWarning("UpdateCard failed | CardId: {CardId} | UserId: {UserId} | Reason: Card not found", cardId, userId);
                        return Result<bool>.Failure("Card not found or access denied.");
                    }

                    var accountExists = await _cardRepo.AccountExistsAsync(dto.AccountId, userId);
                    if (!accountExists)
                    {
                        _logger.LogWarning("UpdateCard failed | CardId: {CardId} | UserId: {UserId} | Reason: Account not found", cardId, userId);
                        return Result<bool>.Failure("Linked account not found or access denied.");
                    }

                    card.CardName = dto.CardName;
                    card.AccountId = dto.AccountId;

                    await _cardRepo.UpdateAsync(card);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    _logger.LogInformation("UpdateCard successful | CardId: {CardId} | UserId: {UserId}", cardId, userId);
                    return Result<bool>.Success(true);
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogError(ex, "UpdateCard failed | CardId: {CardId} | UserId: {UserId}", cardId, userId);
                    return Result<bool>.Failure("An error occurred while updating the card.");
                }
            });
        }
        public async Task<Result<bool>> UpdateCardStatusAsync(Guid cardId, Guid userId, CardStatusUpdateRequest dto)
        {
            var strategy = _unitOfWork.DbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    var card = await _cardRepo.GetCardByIdAsync(cardId, userId);
                    if (card == null)
                    {
                        _logger.LogWarning("UpdateCardStatus failed | CardId: {CardId} | UserId: {UserId} | Reason: Card not found", cardId, userId);
                        return Result<bool>.Failure("Card not found or access denied.");
                    }

                    var previousStatus = card.Status;
                    card.Status = dto.Status;

                    await _cardRepo.UpdateAsync(card);

                    var audit = new CardAuditLog
                    {
                        CardId = cardId,
                        UserId = userId,
                        PreviousStatus = previousStatus,
                        NewStatus = dto.Status,
                        Reason = dto.Reason,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _cardRepo.LogAuditAsync(audit);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    _logger.LogInformation("UpdateCardStatus successful | CardId: {CardId} | UserId: {UserId} | NewStatus: {NewStatus}", cardId, userId, dto.Status);
                    return Result<bool>.Success(true);
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogError(ex, "UpdateCardStatus failed | CardId: {CardId} | UserId: {UserId}", cardId, userId);
                    return Result<bool>.Failure("An error occurred while updating card status.");
                }
            });
        }
        public async Task<Result<bool>> DeleteCardAsync(Guid cardId, Guid userId)
        {
            var strategy = _unitOfWork.DbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    var card = await _cardRepo.GetCardByIdAsync(cardId, userId);
                    if (card == null)
                    {
                        _logger.LogWarning("DeleteCard failed | CardId: {CardId} | UserId: {UserId} | Reason: Card not found", cardId, userId);
                        return Result<bool>.Failure("Card not found or access denied.");
                    }

                    var previousStatus = card.Status;
                    card.Status = CardStatus.Cancelled;
                    card.IsDeleted = true;
                    card.CardNumber = string.Empty;
                    card.CVV = string.Empty;
                    card.Last4Digits = string.Empty;

                    await _cardRepo.UpdateAsync(card);

                    var audit = new CardAuditLog
                    {
                        CardId = cardId,
                        UserId = userId,
                        PreviousStatus = previousStatus,
                        NewStatus = CardStatus.Cancelled,
                        Reason = "Card deleted by user",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsDeleted = true
                    };

                    await _cardRepo.LogAuditAsync(audit);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    _logger.LogInformation("DeleteCard successful | CardId: {CardId} | UserId: {UserId}", cardId, userId);
                    return Result<bool>.Success(true);
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogError(ex, "DeleteCard failed | CardId: {CardId} | UserId: {UserId}", cardId, userId);
                    return Result<bool>.Failure("An error occurred while deleting the card.");
                }
            });
        }
        public async Task<CardLimitsResponseDto?> GetCardLimitsAsync(Guid cardId, Guid userId)
        {
            var card = await _cardRepo.GetCardByIdAsync(cardId, userId);
            if (card == null) return null;

            var dailyUsage = await _cardRepo.GetCardDailyUsageAsync(cardId);
            var monthlyUsage = await _cardRepo.GetCardMonthlyUsageAsync(cardId);

            return new CardLimitsResponseDto
            {
                DailyLimit = card.DailyLimit,
                MonthlyLimit = card.MonthlyLimit,
                DailyUsage = dailyUsage,
                MonthlyUsage = monthlyUsage
            };
        }
        public async Task<Result<bool>> UpdateCardLimitsAsync(Guid cardId, Guid userId, UpdateCardLimitsRequest dto)
        {
            var strategy = _unitOfWork.DbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    var card = await _cardRepo.GetCardByIdAsync(cardId, userId);
                    if (card == null)
                    {
                        _logger.LogWarning("UpdateCardLimits failed | CardId: {CardId} | UserId: {UserId} | Reason: Card not found", cardId, userId);
                        return Result<bool>.Failure("Card not found or access denied.");
                    }

                    if (dto.DailyLimit < 0 || dto.MonthlyLimit < 0)
                    {
                        _logger.LogWarning("UpdateCardLimits failed | CardId: {CardId} | UserId: {UserId} | Reason: Negative limits", cardId, userId);
                        return Result<bool>.Failure("Limits must be non-negative.");
                    }

                    card.DailyLimit = dto.DailyLimit;
                    card.MonthlyLimit = dto.MonthlyLimit;

                    await _cardRepo.UpdateAsync(card);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    _logger.LogInformation("UpdateCardLimits successful | CardId: {CardId} | UserId: {UserId}", cardId, userId);
                    return Result<bool>.Success(true);
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogError(ex, "UpdateCardLimits failed | CardId: {CardId} | UserId: {UserId}", cardId, userId);
                    return Result<bool>.Failure("An error occurred while updating card limits.");
                }
            });
        }
    }
}
