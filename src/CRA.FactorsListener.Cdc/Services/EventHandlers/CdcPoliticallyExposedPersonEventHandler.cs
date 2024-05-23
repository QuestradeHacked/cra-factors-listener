using System.Text.Json;
using System.Threading.Tasks;
using CRA.FactorsListener.Cdc.Models;
using CRA.FactorsListener.Cdc.Models.Events;
using CRA.FactorsListener.Cdc.Services.UserMapping;
using Microsoft.Extensions.Logging;
using Questrade.Library.PubSubClientHelper.Primitives;

namespace CRA.FactorsListener.Cdc.Services.EventHandlers
{
    public class CdcPoliticallyExposedPersonEventHandler : ICdcEventHandler<PoliticallyExposedPerson>
    {
        private readonly IUserMappingsService _userMappingsService;
        private readonly IPublisherService<PoliticallyExposedPersonChanged> _pubSubPublisher;
        private readonly ILogger<CdcPoliticallyExposedPersonEventHandler> _logger;

        public CdcPoliticallyExposedPersonEventHandler(
            IUserMappingsService userMappingsService,
            IPublisherService<PoliticallyExposedPersonChanged> pubSubPublisher,
            ILogger<CdcPoliticallyExposedPersonEventHandler> logger)
        {
            _userMappingsService = userMappingsService;
            _pubSubPublisher = pubSubPublisher;
            _logger = logger;
        }

        public async Task ProcessAsync(CdcEvent<PoliticallyExposedPerson> message)
        {
            var politicallyExposedPerson = message.GetLastSnapshot();

            _logger.BeginScope(
                "Triggering rating recalculation for PoliticallyExposedPerson: {PoliticallyExposedPersonId}, PersonId: {PersonId}",
                politicallyExposedPerson.PoliticallyExposedPersonId,
                politicallyExposedPerson.PersonId);

            if (message.IsDeleted())
            {
                _logger.LogDebug(
                    "Unsupported operation type: {Operation}",
                    message.Operation);

                return;
            }

            if (message.IsUpdated() &&
                message.After.IsPoliticallyExposed == message.Before?.IsPoliticallyExposed)
            {
                _logger.LogDebug("Ignore: information whether person is politically exposed have to be changed");

                return;
            }

            var userId = await _userMappingsService.GetUserByPersonAsync(message.After.PersonId);

            if (userId == null) return;

            var politicallyExposedPersonChangedEnvelope =
                new PoliticallyExposedPersonChangedEnvelope(userId, message.TransactionTimestamp)
                {
                    IsPoliticallyExposed = message.After.IsPoliticallyExposed != 0
                };

            var politicallyExposedPersonChanged =
                new PoliticallyExposedPersonChanged(politicallyExposedPersonChangedEnvelope);

            _logger.LogDebug(
                "Published message for user: {UserId}, isPoliticallyExposed: {IsPoliticallyExposed}", 
                userId, message.After.IsPoliticallyExposed);
    
            await _pubSubPublisher.PublishMessageAsync(politicallyExposedPersonChanged);
        }

    }
}
