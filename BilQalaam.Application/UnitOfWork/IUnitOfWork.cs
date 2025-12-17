using BilQalaam.Application.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace BilQalaam.Application.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<T> Repository<T>() where T : class;

        Task<int> CompleteAsync();
        
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}
