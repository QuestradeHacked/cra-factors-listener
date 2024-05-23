using System.Threading;
using System.Threading.Tasks;
using CRA.FactorsListener.Domain.Entities;

namespace CRA.FactorsListener.Cdc.Services.UserMapping
{
    public interface IUserAccountService
    {
        Task<UserAccount> GetByAccountId(string accountId, CancellationToken token = default);
    }
}