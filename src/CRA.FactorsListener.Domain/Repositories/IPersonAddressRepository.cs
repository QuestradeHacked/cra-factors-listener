using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRA.FactorsListener.Domain.Entities;

namespace CRA.FactorsListener.Domain.Repositories
{
    public interface IPersonAddressRepository : IBaseRepository<PersonAddress>
    {
        Task<IList<PersonAddress>> GetByAddressIdAndType(
            string addressId,
            int addressType,
            CancellationToken cancellationToken = default);
    }
}
