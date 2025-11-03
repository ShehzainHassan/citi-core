using citi_core.Data;

namespace citi_core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        IAuthRepository AuthRepository { get; }
        IAccountRepository AccountRepository { get; }
        ITransactionRepository TransactionRepository { get; }
        IBeneficiaryRepository BeneficiaryRepository { get; }
        ICardRepository CardRepository { get; }
        ApplicationDbContext DbContext { get; }
    }
}
