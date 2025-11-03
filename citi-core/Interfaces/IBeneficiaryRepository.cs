using citi_core.Models;

public interface IBeneficiaryRepository
{
    Task<List<Beneficiary>> GetBeneficiaryByUserIdAsync(Guid userId);
    Task<Beneficiary?> GetBeneficiaryByIdAsync(Guid beneficiaryId, Guid userId);
    Task<bool> ExistsAsync(Guid userId, string accountNumber);
    Task AddBeneficiaryAsync(Beneficiary beneficiary);
    Task UpdateBeneficiaryAsync(Beneficiary beneficiary);
    Task DeleteBeneficiaryAsync(Beneficiary beneficiary);
}