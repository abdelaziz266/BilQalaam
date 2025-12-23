using BilQalaam.Application.Repositories.Interfaces;
using BilQalaam.Infrastructure.Persistence;
using BilQalaam.Domain.Common;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BilQalaam.Infrastructure.Repositories.Implementations
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly BilQalaamDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(BilQalaamDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            var query = _dbSet.AsNoTracking();
            
            // تصفية البيانات المحذوفة إذا كانت Entity ترث من Base
            if (typeof(Base).IsAssignableFrom(typeof(T)))
            {
                query = query.Where(x => !((Base)(object)x).IsDeleted);
            }
            
            return await query.ToListAsync();
        }

        public async Task<T?> GetByIdAsync(object id)
        {
            var entity = await _dbSet.FindAsync(id);
            
            // التحقق من أن الـ entity لم يتم حذفه
            if (entity != null && typeof(Base).IsAssignableFrom(typeof(T)))
            {
                var baseEntity = (Base)(object)entity;
                if (baseEntity.IsDeleted)
                    return null;
            }
            
            return entity;
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            var query = _dbSet.Where(predicate).AsNoTracking();
            
            // تصفية البيانات المحذوفة إذا كانت Entity ترث من Base
            if (typeof(Base).IsAssignableFrom(typeof(T)))
            {
                query = query.Where(x => !((Base)(object)x).IsDeleted);
            }
            
            return await query.ToListAsync();
        }

        public async Task AddAsync(T entity)
            => await _dbSet.AddAsync(entity);

        public void Update(T entity)
            => _dbSet.Update(entity);

        public void Delete(T entity)
            => _dbSet.Remove(entity);
        public IQueryable<T> Query()
        {
            var query = _dbSet.AsNoTracking().AsQueryable();

            if (typeof(Base).IsAssignableFrom(typeof(T)))
            {
                query = query.Where(x => !((Base)(object)x).IsDeleted);
            }

            return query;
        }

    }
}
