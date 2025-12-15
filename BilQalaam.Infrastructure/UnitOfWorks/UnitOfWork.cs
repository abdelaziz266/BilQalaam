
using BilQalaam.Application.UnitOfWork;
using BilQalaam.Infrastructure.Persistence;

namespace BilQalaam.Infrastructure.UnitOfWorks
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly BilQalaamDbContext _context;

        public UnitOfWork(BilQalaamDbContext context)
        {
            _context = context;
        }

        public async Task<int> CompleteAsync() => await _context.SaveChangesAsync();

        public void Dispose() => _context.Dispose();
    }
}
