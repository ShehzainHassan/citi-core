using BCrypt.Net;
using citi_core.Common;
using citi_core.Common.citi_core.Common;
using citi_core.Interfaces;
using citi_core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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

        public async Task<Result<User>> AddUserAsync(User user)
        {
            var strategy = _unitOfWork.DbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    if (string.IsNullOrWhiteSpace(user.Email))
                        return Result<User>.Failure("Email is required.");

                    if (string.IsNullOrWhiteSpace(user.FullName))
                        return Result<User>.Failure("Full name is required.");

                    if (string.IsNullOrWhiteSpace(user.PasswordHash))
                        return Result<User>.Failure("Password is required.");

                    var existingUserByEmail = await _unitOfWork.Users.GetByEmailAsync(user.Email);
                    if (existingUserByEmail != null)
                        return Result<User>.Failure("Email already exists.");

                    if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
                    {
                        var existingUserByPhone = await _unitOfWork.Users.GetByPhoneAsync(user.PhoneNumber);
                        if (existingUserByPhone != null)
                            return Result<User>.Failure("Phone number already exists.");
                    }

                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash, workFactor: 12);

                    await _unitOfWork.Users.AddUserAsync(user);
                    await _unitOfWork.SaveChangesAsync();

                    await _unitOfWork.CommitTransactionAsync();

                    return Result<User>.Success(user);
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogError(ex, "Failed to add user. Email={Email}", user.Email);
                    return Result<User>.Failure("Failed to add user.");
                }
            });
        }
    }
}
