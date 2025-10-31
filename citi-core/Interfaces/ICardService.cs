using citi_core.Common.citi_core.Common;
using citi_core.Dto;
using citi_core.Models;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace citi_core.Interfaces
{
    public interface ICardService
    {
        Task<List<CardDto>> GetUserCardsAsync(Guid userId);
        Task<CardDetailsDto?> GetCardDetailsAsync(Guid cardId, Guid userId);
        Task<Result<AddCardRequestDto>> AddCardRequestAsync(Guid userId, AddCardRequest request);
        Task<Result<bool>> UpdateCardAsync(Guid cardId, Guid userId, UpdateCardRequest dto);
        Task<Result<bool>> UpdateCardStatusAsync(Guid cardId, Guid userId, CardStatusUpdateRequest dto);
        Task<Result<bool>> DeleteCardAsync(Guid cardId, Guid userId);
        Task<CardLimitsResponseDto?> GetCardLimitsAsync(Guid cardId, Guid userId);
        Task<Result<bool>> UpdateCardLimitsAsync(Guid cardId, Guid userId, UpdateCardLimitsRequest dto);
    }
}
