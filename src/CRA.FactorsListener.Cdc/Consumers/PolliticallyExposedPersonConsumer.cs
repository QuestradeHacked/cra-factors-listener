using System.Threading.Tasks;
using AutoMapper;
using cdc.customer_risk_assessment.crm.dbo.PoliticallyExposedPerson;
using Confluent.SchemaRegistry;
using CRA.FactorsListener.Cdc.Models;
using CRA.FactorsListener.Cdc.Services.EventHandlers;
using CRA.FactorsListener.Cdc.Services.Metrics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Questrade.Library.Kafka.AspNetCore;
using Questrade.Library.Kafka.AspNetCore.Configuration;
using StatsdClient;

namespace CRA.FactorsListener.Cdc.Consumers
{
    public class PoliticallyExposedPersonConsumer : BaseAvroConsumer<Key, Envelope>
    {
        private readonly IMapper _mapper;
        private readonly ICdcEventHandler<PoliticallyExposedPerson> _politicallyExposedPersonEnvelopeService;
        
        public PoliticallyExposedPersonConsumer(
            ILogger<PoliticallyExposedPersonConsumer> logger,
            KafkaConfiguration kafkaConfiguration,
            IMemoryCache memoryCache,
            IDogStatsd dogStatsdService,
            SchemaRegistryConfig schemaRegistryConfig,
            IMapper mapper,
            ICdcEventHandler<PoliticallyExposedPerson> politicallyExposedPersonEnvelopeService,
            IMetricService metricService,
            KafkaConsumerBuilder<Key, Envelope> consumerBuilder = null)
            : base(logger, kafkaConfiguration, memoryCache, dogStatsdService, schemaRegistryConfig, metricService, consumerBuilder)
        {
            _mapper = mapper;
            _politicallyExposedPersonEnvelopeService = politicallyExposedPersonEnvelopeService;
        }

        public override string ConsumerName => nameof(PoliticallyExposedPersonConsumer);

        protected override Task ProcessMessageAsync(KafkaConsumerMessage<Key, Envelope> e)
        {
            var message = _mapper.Map<Envelope, CdcEvent<PoliticallyExposedPerson>>(e.Value);
            
            return _politicallyExposedPersonEnvelopeService.ProcessAsync(message);
        }
    }
}
