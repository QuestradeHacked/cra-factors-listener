using System.Threading;
using System.Threading.Tasks;
using CRA.FactorsListener.Domain.Entities;

namespace CRA.FactorsListener.Cdc.Services.UserMapping
{
    public interface IUserService
    {
        Task<User> GetByPersonId(string personId, CancellationToken token = default);
    }
}