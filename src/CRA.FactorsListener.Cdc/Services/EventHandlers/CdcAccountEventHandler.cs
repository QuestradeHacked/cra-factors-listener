using System.Text.Json;
using System.Threading.Tasks;
using CRA.FactorsListener.Cdc.Extensions;
using CRA.FactorsListener.Cdc.Models;
using CRA.FactorsListener.Cdc.Models.Accounts;
using CRA.FactorsListener.Cdc.Models.Enums;
using CRA.FactorsListener.Cdc.Models.Events;
using CRA.FactorsListener.Cdc.Options;
using CRA.FactorsListener.Cdc.Services.UserMapping;
using Microsoft.Extensions.Logging;
using Questrade.Library.PubSubClientHelper.Primitives;

namespace CRA.FactorsListener.Cdc.Services.EventHandlers
{
    public class CdcAccountEventHandler : ICdcEventHandler<Account>
    {
        private readonly IUserMappingsService _userMappingsService;
        private readonly RiskFactorsOptions _riskFactorsOptions;
        private readonly IPublisherService<AccountChanged> _pubSubPublisher;
        private readonly ILogger<CdcAccountEventHandler> _logger;

        public CdcAccountEventHandler(
            IUserMappingsService userMappingsService,
            RiskFactorsOptions riskFactorsOptions,
            IPublisherService<AccountChanged> pubSubPublisher,
            ILogger<CdcAccountEventHandler> logger)
        {
            _userMappingsService = userMappingsService;
            _riskFactorsOptions = riskFactorsOptions;
            _pubSubPublisher = pubSubPublisher;
            _logger = logger;
        }

        public async Task ProcessAsync(CdcEvent<Account> message)
        {
            _logger.BeginScope(
                "Triggering rating recalculation for Account: {AccountId}",
                message.GetLastSnapshot().AccountId);

            if (message.IsDeleted())
            {
                _logger.LogDebug(
                    "Unsupported operation type: {Operation}",
                    message.Operation);

                return;
            }

            if (message.IsNewRecord() && GetAccountStatus(message.After.AccountStatusId) == AccountStatus.Inactive)
            {
                _logger.LogDebug("Ignore new record: inactive account");
                return;
            }

            if (message.IsUpdated() && GetAccountStatus(message.Before.AccountStatusId) ==
                GetAccountStatus(message.After.AccountStatusId))
            {
                _logger.LogDebug("Ignore: account status have to become active or inactive");
                return;
            }

            if (GetAccountStatus(message.GetLastSnapshot().AccountStatusId) == AccountStatus.Unknown)
            {
                _logger.LogDebug("Unknown account type was received");
                return;
            }

            var userId = await _userMappingsService.GetUserByAccountAsync(message.After.AccountId);

            if (userId == null)
            {
                throw new UserNotFoundException(message.After);
            }

            var accountChangedEnvelope = new AccountChangedEnvelope(userId, message.After.AccountId, message.TransactionTimestamp)
            {
                AccountStatus = GetAccountStatus(message.After.AccountStatusId).ToString()
            };

            _logger.LogInformation(
                "Received new account message: {Message}",
                JsonSerializer.Serialize(accountChangedEnvelope));

            var accountChanged = new AccountChanged(accountChangedEnvelope);

            _logger.LogDebug(
                "Published message for Account: {AccountId}", 
                message.After.AccountId);

            await _pubSubPublisher.PublishMessageAsync(accountChanged);
        }


        private AccountStatus GetAccountStatus(string accountType)
        {
            if (string.IsNullOrEmpty(accountType)) return AccountStatus.Unknown;

            return _riskFactorsOptions.ActiveAccountStatusIds.Contains(accountType)
                ? AccountStatus.Active
                : AccountStatus.Inactive;
        }
    }
}
