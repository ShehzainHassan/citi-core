using citi_core.Common.citi_core.Common;
using citi_core.Dto;
using citi_core.Interfaces;
using citi_core.Models;
using Microsoft.EntityFrameworkCore;

public class BeneficiaryService : IBeneficiaryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BeneficiaryService> _logger;
    public BeneficiaryService(IUnitOfWork unitOfWork, ILogger<BeneficiaryService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    public async Task<List<Beneficiary>> GetBeneficiariesAsync(Guid userId)
    {
        return await _unitOfWork.BeneficiaryRepository.GetBeneficiaryByUserIdAsync(userId);
    }
    public async Task<Beneficiary?> GetBeneficiaryByIdAsync(Guid beneficiaryId, Guid userId)
    {
        return await _unitOfWork.BeneficiaryRepository.GetBeneficiaryByIdAsync(beneficiaryId, userId);
    }
    public async Task<Result<Guid>> AddBeneficiaryAsync(Guid userId, AddBeneficiaryRequest request)
    {
        var strategy = _unitOfWork.DbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var exists = await _unitOfWork.BeneficiaryRepository.ExistsAsync(userId, request.AccountNumber);
                if (exists)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result<Guid>.Failure("Beneficiary already exists");
                }

                var beneficiary = new Beneficiary
                {
                    BeneficiaryId = Guid.NewGuid(),
                    UserId = userId,
                    BeneficiaryName = request.BeneficiaryName,
                    AccountNumber = request.AccountNumber,
                    BankName = request.BankName,
                    BankCode = request.BankCode,
                    PhoneNumber = request.PhoneNumber,
                    Email = request.Email,
                    Nickname = request.Nickname
                };

                await _unitOfWork.BeneficiaryRepository.AddBeneficiaryAsync(beneficiary);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return Result<Guid>.Success(beneficiary.BeneficiaryId);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "AddBeneficiary failed | UserId: {UserId}", userId);
                return Result<Guid>.Failure("An error occurred while adding the beneficiary.");
            }
        });
    }
    public async Task<Result<bool>> UpdateBeneficiaryAsync(Guid beneficiaryId, Guid userId, UpdateBeneficiaryRequest request)
    {
        var strategy = _unitOfWork.DbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var beneficiary = await _unitOfWork.BeneficiaryRepository.GetBeneficiaryByIdAsync(beneficiaryId, userId);
                if (beneficiary == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result<bool>.Failure("Beneficiary not found");
                }

                beneficiary.BeneficiaryName = request.BeneficiaryName;
                beneficiary.BankName = request.BankName;
                beneficiary.BankCode = request.BankCode;
                beneficiary.PhoneNumber = request.PhoneNumber;
                beneficiary.Email = request.Email;
                beneficiary.Nickname = request.Nickname;

                await _unitOfWork.BeneficiaryRepository.UpdateBeneficiaryAsync(beneficiary);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "UpdateBeneficiary failed | BeneficiaryId: {BeneficiaryId} | UserId: {UserId}", beneficiaryId, userId);
                return Result<bool>.Failure("An error occurred while updating the beneficiary.");
            }
        });
    }
    public async Task<Result<bool>> DeleteBeneficiaryAsync(Guid beneficiaryId, Guid userId)
    {
        var strategy = _unitOfWork.DbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var beneficiary = await _unitOfWork.BeneficiaryRepository.GetBeneficiaryByIdAsync(beneficiaryId, userId);
                if (beneficiary == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result<bool>.Failure("Beneficiary not found");
                }

                await _unitOfWork.BeneficiaryRepository.DeleteBeneficiaryAsync(beneficiary);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "DeleteBeneficiary failed | BeneficiaryId: {BeneficiaryId} | UserId: {UserId}", beneficiaryId, userId);
                return Result<bool>.Failure("An error occurred while deleting the beneficiary.");
            }
        });
    }
    public async Task<Result<bool>> SetFavoriteAsync(Guid beneficiaryId, Guid userId, bool isFavorite)
    {
        var beneficiary = await _unitOfWork.BeneficiaryRepository.GetBeneficiaryByIdAsync(beneficiaryId, userId);
        if (beneficiary == null)
            return Result<bool>.Failure("Beneficiary not found");

        beneficiary.IsFavorite = isFavorite;
        await _unitOfWork.BeneficiaryRepository.UpdateBeneficiaryAsync(beneficiary);
        return Result<bool>.Success(true);
    }
}