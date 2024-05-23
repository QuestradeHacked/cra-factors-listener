using System.Threading.Tasks;
using AutoMapper;
using cdc.customer_risk_assessment.crm.dbo.Users;
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
    public class UserConsumer : BaseAvroConsumer<Key, Envelope>
    {
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;

        public UserConsumer(
            ILogger<UserConsumer> logger,
            KafkaConfiguration kafkaConfiguration,
            IMemoryCache memoryCache,
            IDogStatsd dogStatsdService,
            SchemaRegistryConfig schemaRegistryConfig,
            IMapper mapper,
            IUserRepository userRepository,
            IMetricService metricService,
            KafkaConsumerBuilder<Key, Envelope> consumerBuilder = null)
            : base(logger, kafkaConfiguration, memoryCache, dogStatsdService, schemaRegistryConfig, metricService, consumerBuilder)
        {
            _mapper = mapper;
            _userRepository = userRepository;
        }

        public override string ConsumerName => nameof(UserConsumer);

        protected override async Task ProcessMessageAsync(KafkaConsumerMessage<Key, Envelope> e)
        {
            var data = e.Value;

            var message = _mapper.Map<Envelope, CdcEvent<User>>(data);

            if (message.IsNewRecord())
            {
                Logger.LogDebug(
                    "New record: Operation: {Operation}, Key: {User}",
                    message.Operation,
                    e.Key.UserID);
                
                await _userRepository.UpsertAsync(message.After);
            }
            else
            {
                Logger.LogDebug(
                    "Unsupported operation: {Operation}, Key: {User}",
                    message.Operation,
                    e.Key.UserID);
            }
        }
    }
}
