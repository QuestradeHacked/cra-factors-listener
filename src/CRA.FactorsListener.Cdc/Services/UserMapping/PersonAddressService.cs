using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CRA.FactorsListener.Cdc.Extensions;
using CRA.FactorsListener.Cdc.Options;
using CRA.FactorsListener.Cdc.Providers;
using CRA.FactorsListener.Domain.Entities;
using CRA.FactorsListener.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace CRA.FactorsListener.Cdc.Services.UserMapping
{
    public class PersonAddressService : IPersonAddressService
    {
        private readonly ICustomerMasterDataProvider _customerMasterDataProvider;
        private readonly ILogger<PersonAddressService> _logger;
        private readonly IPersonAddressRepository _personAddressRepository;
        private readonly RiskFactorsOptions _riskFactorsOptions;

        public PersonAddressService(
            ICustomerMasterDataProvider customerMasterDataProvider,
            ILogger<PersonAddressService> logger,
            IPersonAddressRepository personAddressRepository,
            RiskFactorsOptions riskFactorsOptions)
        {
            _customerMasterDataProvider = customerMasterDataProvider;
            _logger = logger;
            _personAddressRepository = personAddressRepository;
            _riskFactorsOptions = riskFactorsOptions;
        }

        public async Task<IList<PersonAddress>> GetPrimaryAddressesByAddressId(
            string addressId,
            CancellationToken token = default)
        {
            var personAddresses = await _personAddressRepository.GetByAddressIdAndType(addressId,
                    _riskFactorsOptions.PrimaryResidenceType, token);

            if (!personAddresses.IsNullOrEmpty())
            {
                return personAddresses;
            }

            personAddresses = await _customerMasterDataProvider.GetPersonsByAddressIdAsync(addressId, token);

            if (personAddresses.IsNullOrEmpty())
            {
                _logger.LogInformation("Failed to find personAddress by addressId: {Id}", addressId);
                return null;
            }

            var tasks = personAddresses.Where(x =>
                    x.AddressType == _riskFactorsOptions.PrimaryResidenceType)
                .Select(x => _personAddressRepository.UpsertAsync(x, token));

            await Task.WhenAll(tasks);

            return personAddresses;
        }
    }
}