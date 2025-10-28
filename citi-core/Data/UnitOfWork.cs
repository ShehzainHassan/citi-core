using citi_core.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace citi_core.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;
        public ApplicationDbContext DbContext => _context;
        public IAuthRepository AuthRepository { get; }
        public UnitOfWork(ApplicationDbContext context, IAuthRepository authRepository)
        {
            _context = context;
            AuthRepository = authRepository;
        }
        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
        public async Task BeginTransactionAsync() =>
            _transaction = await _context.Database.BeginTransactionAsync();
        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
            }
        }
        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
            }
        }
        public void Dispose() => _context.Dispose();
    }
}
