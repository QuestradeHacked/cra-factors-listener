using System.Threading.Tasks;
using AutoMapper;
using cdc.customer_risk_assessment.crm.dbo.PersonEmployment;
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
    public class PersonEmploymentConsumer : BaseAvroConsumer<Key, Envelope>
    {
        private readonly IMapper _mapper;
        private readonly ICdcEventHandler<PersonEmployment> _personEmploymentEnvelopeService;
        
        public PersonEmploymentConsumer(
            ILogger<PersonEmploymentConsumer> logger,
            KafkaConfiguration kafkaConfiguration,
            IMemoryCache memoryCache,
            IDogStatsd dogStatsdService,
            SchemaRegistryConfig schemaRegistryConfig,
            IMapper mapper,
            ICdcEventHandler<PersonEmployment> personEmploymentEnvelopeService,
            IMetricService metricService,
            KafkaConsumerBuilder<Key, Envelope> consumerBuilder = null)
            : base(logger, kafkaConfiguration, memoryCache, dogStatsdService, schemaRegistryConfig, metricService, consumerBuilder)
        {
            _mapper = mapper;
            _personEmploymentEnvelopeService = personEmploymentEnvelopeService;
        }

        public override string ConsumerName => nameof(PersonEmploymentConsumer);

        protected override Task ProcessMessageAsync(KafkaConsumerMessage<Key, Envelope> e)
        {
            var message = _mapper.Map<Envelope, CdcEvent<PersonEmployment>>(e.Value);
            
            return _personEmploymentEnvelopeService.ProcessAsync(message);
        }
    }
}
