
using BilQalaam.Application.Repositories.Interfaces;

namespace BilQalaam.Application.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<T> Repository<T>() where T : class;

        Task<int> CompleteAsync();
    }
}
