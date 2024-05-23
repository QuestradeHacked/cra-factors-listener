using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CRA.FactorsListener.DAL.Clients;
using CRA.FactorsListener.Domain.Entities;
using CRA.FactorsListener.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace CRA.FactorsListener.DAL.Repositories
{
    public class PersonAddressRepository : BaseRepository<PersonAddress>, IPersonAddressRepository
    {
        private readonly ILogger<PersonAddressRepository> _logger;

        public PersonAddressRepository(
            FirestoreClientFactory<PersonAddress> firestoreClientFactory,
            ILogger<PersonAddressRepository> logger)
            : base(firestoreClientFactory, "PersonAddresses")
        {
            _logger = logger;
        }

        public async Task<IList<PersonAddress>> GetByAddressIdAndType(
            string addressId,
            int addressType,
            CancellationToken cancellationToken = default)
        {
            var storedPersonAddress =
                await GetByAsync(e => e.AddressId, addressId, cancellationToken);

            var addressByType = storedPersonAddress
                .Where(pa => pa.AddressType == addressType)
                .ToList();

            return addressByType;
        }

        public override async Task<string> UpsertAsync(
            PersonAddress entity,
            CancellationToken cancellationToken = default)
        {
            var storedPersonAddress =
                (await GetByAddressIdAndType(entity.AddressId, entity.AddressType, cancellationToken))
                .FirstOrDefault(address => address.PersonId == entity.PersonId);

            if (storedPersonAddress == null)
            {
                return await base.UpsertAsync(entity, cancellationToken);
            }

            _logger.LogInformation("PersonAddress #{PersonAddressId} is already saved", entity.AddressId);
            return storedPersonAddress.Id;
        }
    }
}
