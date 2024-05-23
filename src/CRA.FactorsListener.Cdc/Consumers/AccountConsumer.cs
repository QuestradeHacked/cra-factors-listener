using AutoMapper;
using cdc.customer_risk_assessment.crm.dbo.Account;
using Confluent.SchemaRegistry;
using CRA.FactorsListener.Cdc.Models;
using CRA.FactorsListener.Cdc.Models.Accounts;
using CRA.FactorsListener.Cdc.Services.EventHandlers;
using CRA.FactorsListener.Cdc.Services.Metrics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Questrade.Library.Kafka.AspNetCore;
using Questrade.Library.Kafka.AspNetCore.Configuration;
using StatsdClient;
using System.Threading.Tasks;

namespace CRA.FactorsListener.Cdc.Consumers
{
    public class AccountConsumer : BaseAvroConsumer<Key, Envelope>
    {
        private readonly IMapper _mapper; 
        private readonly ICdcEventHandler<Account> _accountEnvelopeService;

        public AccountConsumer(
            ILogger<AccountConsumer> logger,
            KafkaConfiguration kafkaConfiguration,
            IMemoryCache memoryCache,
            IDogStatsd dogStatsdService,
            SchemaRegistryConfig schemaRegistryConfig,
            IMapper mapper,
            ICdcEventHandler<Account> accountEnvelopeService,
            IMetricService metricService,
            KafkaConsumerBuilder<Key, Envelope> consumerBuilder = null)
            : base(logger, kafkaConfiguration, memoryCache, dogStatsdService, schemaRegistryConfig, metricService, consumerBuilder)
        {
            _mapper = mapper;
            _accountEnvelopeService = accountEnvelopeService;
        }

        public override string ConsumerName => nameof(AccountConsumer);

        protected override Task ProcessMessageAsync(KafkaConsumerMessage<Key, Envelope> e)
        {
            var message = _mapper.Map<Envelope, CdcEvent<Account>>(e.Value);
            
            return _accountEnvelopeService.ProcessAsync(message);
        }
    }
}
