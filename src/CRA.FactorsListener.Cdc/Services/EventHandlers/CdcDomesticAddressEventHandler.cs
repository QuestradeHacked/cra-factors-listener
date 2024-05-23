using System.Text.Json;
using System.Threading.Tasks;
using CRA.FactorsListener.Cdc.Extensions;
using CRA.FactorsListener.Cdc.Models;
using CRA.FactorsListener.Cdc.Models.Events;
using CRA.FactorsListener.Cdc.Options;
using CRA.FactorsListener.Cdc.Services.UserMapping;
using Microsoft.Extensions.Logging;
using Questrade.Library.PubSubClientHelper.Primitives;

namespace CRA.FactorsListener.Cdc.Services.EventHandlers
{
    public class CdcDomesticAddressEventHandler : ICdcEventHandler<DomesticAddress>
    {
        private readonly IUserMappingsService _userMappingsService;
        private readonly RiskFactorsOptions _riskFactorsOptions;
        private readonly IPublisherService<CountryChanged> _pubSubPublisher;
        private readonly ILogger<CdcDomesticAddressEventHandler> _logger;

        public CdcDomesticAddressEventHandler(
            IUserMappingsService userMappingsService,
            RiskFactorsOptions riskFactorsOptions,
            IPublisherService<CountryChanged> pubSubPublisher,
            ILogger<CdcDomesticAddressEventHandler> logger)
        {
            _userMappingsService = userMappingsService;
            _riskFactorsOptions = riskFactorsOptions;
            _pubSubPublisher = pubSubPublisher;
            _logger = logger;
        }

        public async Task ProcessAsync(CdcEvent<DomesticAddress> message)
        {
            _logger.BeginScope(
                "Triggering rating recalculation for DomesticAddress: {DomesticAddressId}",
                message.GetLastSnapshot().DomesticAddressId);

            if (!message.IsNewRecord())
            {
                _logger.LogDebug(
                    "Unsupported operation type: {Operation}",
                    message.Operation);
                
                return;
            }

            var isAddressAfterPrimaryResidence =
                await _userMappingsService.IsAddressPrimaryResidenceAsync(message.After.DomesticAddressId);

            if (!isAddressAfterPrimaryResidence)
            {
                _logger.LogDebug("Ignore: address have to be primary residence");
                
                return;
            }

            var users = await _userMappingsService.GetUsersByAddressAsync(message.After.DomesticAddressId);

            if (users.IsNullOrEmpty())
                return;

            foreach (var userId in users)
            {
                var countryChangedEnvelope = new CountryChangedEnvelope(
                    userId,
                    _riskFactorsOptions.DomesticCountryId,
                    message.TransactionTimestamp);

                var countryChanged = new CountryChanged(countryChangedEnvelope);

                _logger.LogDebug(
                    "Published message for user: {UserId}, domesticAddress: {DomesticAddressId}", 
                    userId, message.After.DomesticAddressId);

                await _pubSubPublisher.PublishMessageAsync(countryChanged);
            }

        }
    }
}
