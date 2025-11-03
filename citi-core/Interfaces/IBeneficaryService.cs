using citi_core.Common.citi_core.Common;
using citi_core.Dto;
using citi_core.Models;

namespace citi_core.Interfaces
{
    public interface IBeneficiaryService
    {
        Task<List<Beneficiary>> GetBeneficiariesAsync(Guid userId);
        Task<Beneficiary?> GetBeneficiaryByIdAsync(Guid beneficiaryId, Guid userId);
        Task<Result<Guid>> AddBeneficiaryAsync(Guid userId, AddBeneficiaryRequest request);
        Task<Result<bool>> UpdateBeneficiaryAsync(Guid beneficiaryId, Guid userId, UpdateBeneficiaryRequest request);
        Task<Result<bool>> DeleteBeneficiaryAsync(Guid beneficiaryId, Guid userId);
        Task<Result<bool>> SetFavoriteAsync(Guid beneficiaryId, Guid userId, bool isFavorite);
    }
}
