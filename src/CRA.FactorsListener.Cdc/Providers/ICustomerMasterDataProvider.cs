using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRA.FactorsListener.Domain.Entities;

namespace CRA.FactorsListener.Cdc.Providers
{
    public interface ICustomerMasterDataProvider
    {
        Task<UserAccount> GetUserAccountByAccountIdAsync(string accountId,
            CancellationToken cancellationToken = default);

        Task<User> GetUserByPersonIdAsync(string personId, CancellationToken cancellationToken = default);

        Task<IList<PersonAddress>> GetPersonsByAddressIdAsync(string addressId,
            CancellationToken cancellationToken = default);
    }
}
