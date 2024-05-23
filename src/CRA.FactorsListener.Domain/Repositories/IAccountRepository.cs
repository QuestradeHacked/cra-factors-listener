using System.Threading;
using System.Threading.Tasks;
using CRA.FactorsListener.Domain.Entities;

namespace CRA.FactorsListener.Domain.Repositories
{
    public interface IUserAccountRepository : IBaseRepository<UserAccount>
    {
        Task<UserAccount> GetByAccountId(string accountId, CancellationToken token = default);
    }
}