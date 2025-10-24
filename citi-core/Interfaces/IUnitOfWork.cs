using citi_core.Data;

namespace citi_core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users {  get; }
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();

        ApplicationDbContext DbContext { get; }
    }
}
