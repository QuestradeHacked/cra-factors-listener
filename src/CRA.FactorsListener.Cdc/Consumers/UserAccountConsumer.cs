using System.Threading.Tasks;
using AutoMapper;
using cdc.customer_risk_assessment.crm.dbo.UserAccount;
using Confluent.SchemaRegistry;
using CRA.FactorsListener.Cdc.Models;
using CRA.FactorsListener.Cdc.Services.Metrics;
using CRA.FactorsListener.Domain.Entities;
using CRA.FactorsListener.Domain.Repositories;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Questrade.Library.Kafka.AspNetCore;
using Questrade.Library.Kafka.AspNetCore.Configuration;
using StatsdClient;

namespace CRA.FactorsListener.Cdc.Consumers
{
    public class UserAccountConsumer : BaseAvroConsumer<Key, Envelope>
    {
        private readonly IMapper _mapper;
        private readonly IUserAccountRepository _userAccountRepository;

        public UserAccountConsumer(
            ILogger<UserAccountConsumer> logger,
            KafkaConfiguration kafkaConfiguration,
            IMemoryCache memoryCache,
            IDogStatsd dogStatsdService,
            SchemaRegistryConfig schemaRegistryConfig,
            IMapper mapper, IUserAccountRepository userAccountRepository,
            IMetricService metricService,
            KafkaConsumerBuilder<Key, Envelope> consumerBuilder = null)
            : base(logger, kafkaConfiguration, memoryCache, dogStatsdService, schemaRegistryConfig, metricService, consumerBuilder)
        {
            _mapper = mapper;
            _userAccountRepository = userAccountRepository;
        }

        public override string ConsumerName => nameof(UserAccountConsumer);

        protected override async Task ProcessMessageAsync(KafkaConsumerMessage<Key, Envelope> e)
        {
            var data = e.Value;

            var message = _mapper.Map<Envelope, CdcEvent<UserAccount>>(data);

            if (message.IsNewRecord())
            {
                Logger.LogDebug(
                    "New record: Operation: {Operation}. Composite key: {UserId}:{Account}",
                    message.Operation, e.Key.UserID, e.Key.AccountID);
                
                await _userAccountRepository.UpsertAsync(message.After);
            }
            else
            {
                Logger.LogDebug(
                    "Unsupported operation: {Operation}, Composite key: {User}:{Account}",
                    message.Operation,
                    e.Key.UserID,
                    e.Key.AccountID);
            }
        }
    }
}
