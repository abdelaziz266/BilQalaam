using BilQalaam.Application.Repositories.Interfaces;
using BilQalaam.Application.UnitOfWork;
using BilQalaam.Infrastructure.Persistence;
using BilQalaam.Infrastructure.Repositories.Implementations;

namespace BilQalaam.Infrastructure.UnitOfWorks
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly BilQalaamDbContext _context;
        private readonly Dictionary<Type, object> _repositories = new();

        public UnitOfWork(BilQalaamDbContext context)
        {
            _context = context;
        }

        public IGenericRepository<T> Repository<T>() where T : class
        {
            var type = typeof(T);

            if (!_repositories.ContainsKey(type))
            {
                _repositories[type] = new GenericRepository<T>(_context);
            }

            return (IGenericRepository<T>)_repositories[type];
        }

        public async Task<int> CompleteAsync()
            => await _context.SaveChangesAsync();

        public void Dispose()
            => _context.Dispose();
    }
}
