using System.Threading;
using System.Threading.Tasks;
using CRA.FactorsListener.Domain.Entities;

namespace CRA.FactorsListener.Domain.Repositories
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User> GetByPersonId(string personId, CancellationToken token = default);
    }
}