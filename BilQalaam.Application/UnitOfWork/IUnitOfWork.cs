using System.Threading.Tasks;

namespace BilQalaam.Application.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        Task<int> CompleteAsync();
    }
}
