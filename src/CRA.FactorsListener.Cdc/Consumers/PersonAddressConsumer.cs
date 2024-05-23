using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using cdc.customer_risk_assessment.crm.dbo.PersonAddress;
using Confluent.SchemaRegistry;
using CRA.FactorsListener.Cdc.Models;
using CRA.FactorsListener.Domain.Entities;
using CRA.FactorsListener.Domain.Repositories;
using CRA.FactorsListener.Cdc.Extensions;
using CRA.FactorsListener.Cdc.Models.Enums;
using CRA.FactorsListener.Cdc.Services.Metrics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Questrade.Library.Kafka.AspNetCore;
using Questrade.Library.Kafka.AspNetCore.Configuration;
using StatsdClient;

namespace CRA.FactorsListener.Cdc.Consumers
{
    public class PersonAddressConsumer : BaseAvroConsumer<Key, Envelope>
    {
        private readonly OperationType[] _supportedOperations =
        {
            OperationType.Read, OperationType.Create, OperationType.Update
        };

        private readonly IPersonAddressRepository _personAddressRepository;
        private readonly IMapper _mapper;

        public PersonAddressConsumer(
            ILogger<PersonAddressConsumer> logger,
            KafkaConfiguration kafkaConfiguration,
            IMemoryCache memoryCache,
            IDogStatsd dogStatsdService,
            SchemaRegistryConfig schemaRegistryConfig,
            IMapper mapper,
            IPersonAddressRepository personAddressRepository,
            IMetricService metricService,
            KafkaConsumerBuilder<Key, Envelope> consumerBuilder = null)
            : base(logger, kafkaConfiguration, memoryCache, dogStatsdService, schemaRegistryConfig, metricService,
                consumerBuilder)
        {
            _mapper = mapper;
            _personAddressRepository = personAddressRepository;
        }

        public override string ConsumerName => nameof(PersonAddressConsumer);

        protected override async Task ProcessMessageAsync(KafkaConsumerMessage<Key, Envelope> e)
        {
            var data = e.Value;

            var message = _mapper.Map<Envelope, CdcEvent<PersonAddress>>(data);

            if (!_supportedOperations.Contains(message.Operation))
            {
                Logger.LogDebug(
                    "Unsupported operation type: {Operation}. Key: {Key}",
                    message.Operation,
                    e.Key.PersonAddressID);

                return;
            }

            if (message.IsNewRecord())
            {
                Logger.LogDebug(
                    "New record: Operation: {Operation}. Key: {Key}",
                    message.Operation, e.Key.PersonAddressID);
                
                await _personAddressRepository.UpsertAsync(message.After);
                
                return;
            }

            if (!message.IsValueUpdated())
            {
                Logger.LogDebug("There is no updates on PersonId, AddressId or AddressType. Operation: {Operation}. Key: {Key}",
                message.Operation, e.Key.PersonAddressID);
                
                return;
            }

            var personAddresses = await _personAddressRepository
                .GetByAddressIdAndType(message.Before.AddressId, message.Before.AddressType);

            var storedAddress = personAddresses.FirstOrDefault(pa => pa.PersonId == message.After.PersonId)
                                ?? new PersonAddress();

            storedAddress.AddressType = message.After.AddressType;
            storedAddress.AddressId = message.After.AddressId;
            storedAddress.PersonId = message.After.PersonId;

            await _personAddressRepository.UpsertAsync(storedAddress);
        }
    }
}
