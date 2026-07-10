using GolAhora.Data.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace GolAhora.Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly Dictionary<Type, object> _repositories = new();
        private bool _disposed;

        public UnitOfWork(AppContext context)
        {
            Context = context;
        }

        public AppContext Context { get; }

        public IRepository<TEntity> Repository<TEntity>() where TEntity : class
        {
            var entityType = typeof(TEntity);
            if (_repositories.TryGetValue(entityType, out var repository))
            {
                return (IRepository<TEntity>)repository;
            }

            var newRepository = new Repository<TEntity>(Context);
            _repositories[entityType] = newRepository;
            return newRepository;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await Context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            return await Context.Database.BeginTransactionAsync(cancellationToken);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                Context.Dispose();
            }
            _disposed = true;
        }
    }
}
