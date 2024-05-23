using System.Threading.Tasks;
using AutoMapper;
using cdc.customer_risk_assessment.crm.dbo.DomesticAddress;
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
    public class DomesticAddressConsumer : BaseAvroConsumer<Key, Envelope>
    {
        private readonly IMapper _mapper;
        private readonly ICdcEventHandler<DomesticAddress> _domesticAddressEnvelopeService;

        public DomesticAddressConsumer(
            ILogger<DomesticAddressConsumer> logger,
            KafkaConfiguration kafkaConfiguration,
            IMemoryCache memoryCache,
            IDogStatsd dogStatsdService,
            SchemaRegistryConfig schemaRegistryConfig,
            IMapper mapper,
            ICdcEventHandler<DomesticAddress> domesticAddressEnvelopeService,
            IMetricService metricService,
            KafkaConsumerBuilder<Key, Envelope> consumerBuilder = null)
            : base(logger, kafkaConfiguration, memoryCache, dogStatsdService, schemaRegistryConfig, metricService, consumerBuilder)
        {
            _mapper = mapper;
            _domesticAddressEnvelopeService = domesticAddressEnvelopeService;
        }

        public override string ConsumerName => nameof(DomesticAddressConsumer);

        protected override Task ProcessMessageAsync(KafkaConsumerMessage<Key, Envelope> e)
        {
            var message = _mapper.Map<Envelope, CdcEvent<DomesticAddress>>(e.Value);
            
            return _domesticAddressEnvelopeService.ProcessAsync(message);
        }
    }
}
