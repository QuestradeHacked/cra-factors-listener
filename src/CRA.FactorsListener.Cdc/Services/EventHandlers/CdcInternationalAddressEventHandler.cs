using System.Text.Json;
using System.Threading.Tasks;
using CRA.FactorsListener.Cdc.Extensions;
using CRA.FactorsListener.Cdc.Models;
using CRA.FactorsListener.Cdc.Models.Events;
using CRA.FactorsListener.Cdc.Services.UserMapping;
using Microsoft.Extensions.Logging;
using Questrade.Library.PubSubClientHelper.Primitives;

namespace CRA.FactorsListener.Cdc.Services.EventHandlers
{
    public class CdcInternationalAddressEventHandler : ICdcEventHandler<InternationalAddress>
    {
        private readonly IUserMappingsService _userMappingsService;
        private readonly IPublisherService<CountryChanged> _pubSubPublisher;
        private readonly ILogger<CdcInternationalAddressEventHandler> _logger;

        public CdcInternationalAddressEventHandler(
            IUserMappingsService userMappingsService,
            IPublisherService<CountryChanged> pubSubPublisher,
            ILogger<CdcInternationalAddressEventHandler> logger)
        {
            _userMappingsService = userMappingsService;
            _pubSubPublisher = pubSubPublisher;
            _logger = logger;
        }

        public async Task ProcessAsync(CdcEvent<InternationalAddress> message)
        {
            _logger.BeginScope(
                "Triggering rating recalculation for InternationalAddress: {InternationalAddressId}",
                message.GetLastSnapshot().InternationalAddressId);

            if (message.IsDeleted())
            {
                _logger.LogDebug(
                    "Unsupported operation type: {Operation}",
                    message.Operation);

                return;
            }

            var isAddressAfterPrimaryResidence =
                await _userMappingsService.IsAddressPrimaryResidenceAsync(message.After.InternationalAddressId);

            if (!isAddressAfterPrimaryResidence)
            {
                _logger.LogDebug("Ignore: address have to be primary residence");
                return;
            }

            var isTheSameCountry =
                message.IsUpdated() &&
                await _userMappingsService.IsAddressPrimaryResidenceAsync(message.Before.InternationalAddressId) &&
                message.Before.CountryId == message.After.CountryId;

            if (isTheSameCountry)
            {
                _logger.LogDebug("Ignore: country have to be changed");
                return;
            }

            var users = await _userMappingsService.GetUsersByAddressAsync(message.After.InternationalAddressId);

            if (users.IsNullOrEmpty())
                return;

            foreach (var userId in users)
            {
                var countryChangedEnvelope =
                    new CountryChangedEnvelope(userId, message.After.CountryId, message.TransactionTimestamp);

                var countryChanged = new CountryChanged(countryChangedEnvelope);

                _logger.LogDebug(
                    "Published message for user: {UserId}, internationalAddress: {InternationalAddressId}", 
                    userId, message.After.InternationalAddressId);

                await _pubSubPublisher.PublishMessageAsync(countryChanged);
            }

        }
    }
}
