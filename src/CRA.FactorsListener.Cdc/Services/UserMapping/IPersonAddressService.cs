using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRA.FactorsListener.Domain.Entities;

namespace CRA.FactorsListener.Cdc.Services.UserMapping
{
    public interface IPersonAddressService
    {
        Task<IList<PersonAddress>> GetPrimaryAddressesByAddressId(string addressId, CancellationToken token = default);
    }
}