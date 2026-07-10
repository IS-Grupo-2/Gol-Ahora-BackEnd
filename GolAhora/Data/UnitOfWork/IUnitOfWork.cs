using GolAhora.Data.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace GolAhora.Data.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        AppContext Context { get; }
        IRepository<TEntity> Repository<TEntity>() where TEntity : class;
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    }
}
