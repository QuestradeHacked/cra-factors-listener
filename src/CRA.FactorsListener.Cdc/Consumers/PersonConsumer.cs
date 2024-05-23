using System.Threading.Tasks;
using AutoMapper;
using cdc.customer_risk_assessment.crm.dbo.Persons;
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
    public class PersonConsumer : BaseAvroConsumer<Key, Envelope>
    {
        private readonly IPersonRepository _personRepository;
        private readonly IMapper _mapper;

        public PersonConsumer(
            ILogger<PersonConsumer> logger,
            KafkaConfiguration kafkaConfiguration,
            IMemoryCache memoryCache,
            IDogStatsd dogStatsdService,
            SchemaRegistryConfig schemaRegistryConfig,
            IPersonRepository personRepository,
            IMapper mapper,
            IMetricService metricService,
            KafkaConsumerBuilder<Key, Envelope> consumerBuilder = null)
            : base(logger, kafkaConfiguration, memoryCache, dogStatsdService, schemaRegistryConfig, metricService, consumerBuilder)
        {
            _personRepository = personRepository;
            _mapper = mapper;
        }

        public override string ConsumerName => nameof(PersonConsumer);

        protected override async Task ProcessMessageAsync(KafkaConsumerMessage<Key, Envelope> e)
        {
            var data = e.Value;

            var message = _mapper.Map<Envelope, CdcEvent<Person>>(data);

            if (message.IsNewRecord())
            {
                Logger.LogDebug(
                    "New record: {Operation}, Key: {PersonId}",
                    message.Operation,
                    e.Key.PersonID);
                
                await _personRepository.UpsertAsync(message.After);
            }
            else
            {
                Logger.LogDebug(
                    "Unsupported operation: {Operation}, Key: {PersonId}",
                    message.Operation,
                    e.Key.PersonID);
            }
        }
    }
}
