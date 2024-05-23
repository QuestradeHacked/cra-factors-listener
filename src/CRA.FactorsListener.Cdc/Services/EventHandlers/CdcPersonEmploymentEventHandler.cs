using System.Text.Json;
using System.Threading.Tasks;
using CRA.FactorsListener.Cdc.Models;
using CRA.FactorsListener.Cdc.Models.Events;
using CRA.FactorsListener.Cdc.Services.UserMapping;
using Microsoft.Extensions.Logging;
using Questrade.Library.PubSubClientHelper.Primitives;

namespace CRA.FactorsListener.Cdc.Services.EventHandlers
{
    public class CdcPersonEmploymentEventHandler : ICdcEventHandler<PersonEmployment>
    {
        private readonly IUserMappingsService _userMappingsService;
        private readonly IPublisherService<PersonEmploymentChanged> _pubSubPublisher;
        private readonly ILogger<CdcPersonEmploymentEventHandler> _logger;

        public CdcPersonEmploymentEventHandler(
            IUserMappingsService userMappingsService,
            IPublisherService<PersonEmploymentChanged> pubSubPublisher,
            ILogger<CdcPersonEmploymentEventHandler> logger)
        {
            _userMappingsService = userMappingsService;
            _pubSubPublisher = pubSubPublisher;
            _logger = logger;
        }

        public async Task ProcessAsync(CdcEvent<PersonEmployment> message)
        {
            var personEmployment = message.GetLastSnapshot();

            _logger.BeginScope(
                "Triggering rating recalculation for PersonEmployment: {PersonEmploymentId}, PersonId: {PersonId}",
                personEmployment.PersonEmploymentId,
                personEmployment.PersonId);

            if (message.IsDeleted())
            {
                _logger.LogDebug(
                    "Unsupported operation type: {Operation}",
                    message.Operation);

                return;
            }

            if (message.IsUpdated() && message.Before.Equals(message.After))
            {
                _logger.LogDebug("Ignore: occupation have to be changed");

                return;
            }

            var userId = await _userMappingsService.GetUserByPersonAsync(message.After.PersonId);

            if (userId == null) return;

            var personEmploymentChangedEnvelope =
                new PersonEmploymentChangedEnvelope(userId, message.TransactionTimestamp)
                {
                    Occupation = personEmployment.JobTitle,
                    IsVerified = personEmployment.IsJobTitleVerified,
                    EmploymentType = personEmployment.EmploymentType
                };

            var personEmploymentChanged = new PersonEmploymentChanged(personEmploymentChangedEnvelope);

            _logger.LogDebug(
                "Published message for user: {UserId}, personEmployment: {PersonEmploymentId}", 
                userId, message.After.PersonEmploymentId);
    
            await _pubSubPublisher.PublishMessageAsync(personEmploymentChanged);
        }

    }
}
