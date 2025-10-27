using citi_core.Data;
using citi_core.Dto;
using citi_core.Interfaces;
using citi_core.Models;
using citi_core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class UserServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<ILogger<UserService>> _loggerMock;
    private readonly Mock<ApplicationDbContext> _dbContextMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userRepoMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<UserService>>();
        _dbContextMock = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());

        var dbFacadeMock = new Mock<DatabaseFacade>(_dbContextMock.Object);
        dbFacadeMock.Setup(db => db.CreateExecutionStrategy()).Returns(() => new TestExecutionStrategy());

        _dbContextMock.Setup(db => db.Database).Returns(dbFacadeMock.Object);
        _unitOfWorkMock.Setup(u => u.DbContext).Returns(_dbContextMock.Object);
        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepoMock.Object);

        _userService = new UserService(_unitOfWorkMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task AddUserAsync_ShouldReturnSuccess_WhenUserIsCreated()
    {
        var dto = new CreateUserDto
        {
            Email = "test@example.com",
            FullName = "Test User",
            PhoneNumber = "1234567890",
            Password = "securepassword"
        };

        _userRepoMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync((User)null!);
        _userRepoMock.Setup(r => r.GetByPhoneAsync(dto.PhoneNumber)).ReturnsAsync((User)null!);
        _userRepoMock.Setup(r => r.AddUserAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);

        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _unitOfWorkMock.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);

        var result = await _userService.AddUserAsync(dto);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(dto.Email, result.Value.Email);
        Assert.Equal(dto.FullName, result.Value.FullName);
    }

    [Fact]
    public async Task AddUserAsync_ShouldReturnFailure_WhenEmailExists()
    {
        var dto = new CreateUserDto
        {
            Email = "existing@example.com",
            FullName = "Test User",
            PhoneNumber = "1234567890",
            Password = "securepassword"
        };

        _userRepoMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(new User());

        var result = await _userService.AddUserAsync(dto);

        Assert.False(result.IsSuccess);
        Assert.Equal("Email already exists.", result.ErrorMessage);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task AddUserAsync_ShouldReturnFailure_WhenPhoneExists()
    {
        var dto = new CreateUserDto
        {
            Email = "new@example.com",
            FullName = "Test User",
            PhoneNumber = "1234567890",
            Password = "securepassword"
        };

        _userRepoMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync((User)null!);
        _userRepoMock.Setup(r => r.GetByPhoneAsync(dto.PhoneNumber)).ReturnsAsync(new User());

        var result = await _userService.AddUserAsync(dto);

        Assert.False(result.IsSuccess);
        Assert.Equal("Phone number already exists.", result.ErrorMessage);
        Assert.Null(result.Value);
    }
}

/// <summary>
/// TestExecutionStrategy implements common IExecutionStrategy overloads used by EF Core extension methods.
/// It executes the provided operations directly
/// </summary>
public class TestExecutionStrategy : IExecutionStrategy
{
    public bool RetriesOnFailure => false;

    public TResult Execute<TState, TResult>(TState state, Func<TState, TResult> operation) => operation(state);

    public Task<TResult> ExecuteAsync<TState, TResult>(TState state, Func<TState, CancellationToken, Task<TResult>> operation, CancellationToken cancellationToken = default) => operation(state, cancellationToken);

    public TResult Execute<TState, TResult>(TState state, Func<DbContext, TState, TResult> operation, Func<DbContext, TState, ExecutionResult<TResult>>? verifySucceeded = null)
    {
        return operation(null!, state);
    }

    public async Task<TResult> ExecuteAsync<TState, TResult>(TState state, Func<DbContext, TState, CancellationToken, Task<TResult>> operation, Func<DbContext, TState, CancellationToken, Task<ExecutionResult<TResult>>>? verifySucceeded = null, CancellationToken cancellationToken = default)
    {
        return await operation(null!, state, cancellationToken).ConfigureAwait(false);
    }
}
