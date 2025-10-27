using citi_core.Common.citi_core.Common;
using citi_core.Dto;
using citi_core.Interfaces;
using citi_core.Models;
using Microsoft.EntityFrameworkCore;

namespace citi_core.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserService> _logger;

        public UserService(IUnitOfWork unitOfWork, ILogger<UserService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<User>> AddUserAsync(CreateUserDto dto)
        {
            var strategy = _unitOfWork.DbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync();

                var existingEmail = await _unitOfWork.Users.GetByEmailAsync(dto.Email);
                if (existingEmail != null)
                    return Result<User>.Failure("Email already exists.");

                if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
                {
                    var existingPhone = await _unitOfWork.Users.GetByPhoneAsync(dto.PhoneNumber);
                    if (existingPhone != null)
                        return Result<User>.Failure("Phone number already exists.");
                }

                var user = new User
                {
                    Email = dto.Email,
                    FullName = dto.FullName,
                    PhoneNumber = dto.PhoneNumber,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, 12),
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Users.AddUserAsync(user);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return Result<User>.Success(user);
            });
        }
    }
}
