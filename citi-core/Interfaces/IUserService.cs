using citi_core.Common.citi_core.Common;
using citi_core.Dto;
using citi_core.Models;

namespace citi_core.Interfaces
{
    public interface IUserService
    {
        Task<Result<User>> AddUserAsync(CreateUserDto dto);
    }
}
