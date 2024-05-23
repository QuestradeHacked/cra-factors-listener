using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using CRA.FactorsListener.Cdc.Services.Metrics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Questrade.Library.Kafka.AspNetCore;
using Questrade.Library.Kafka.AspNetCore.Configuration;
using StatsdClient;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CRA.FactorsListener.Cdc.Consumers
{
    public abstract class
        BaseAvroConsumer<TKey, TValue> : KafkaConsumer<TKey, TValue, IDeserializer<TKey>, IDeserializer<TValue>>
    {
        private const string KafkaConsumerStatName = "subscribers.kafka.consume";

        private readonly SchemaRegistryConfig _schemaRegistryConfig;
        private readonly IMetricService _metricService;

        protected BaseAvroConsumer(
            ILogger logger,
            KafkaConfiguration kafkaConfiguration,
            IMemoryCache memoryCache,
            IDogStatsd dogStatsdService,
            SchemaRegistryConfig schemaRegistryConfig,
            IMetricService metricService,
            KafkaConsumerBuilder<TKey, TValue> consumerBuilder = null)
            : base(logger, kafkaConfiguration, memoryCache, dogStatsdService, consumerBuilder)
        {
            _schemaRegistryConfig = schemaRegistryConfig;
            _metricService = metricService;
        }

        protected override IDeserializer<TKey> KeyDeserializer
        {
            get
            {
                var schemaRegistry = new CachedSchemaRegistryClient(_schemaRegistryConfig);

                return new AvroDeserializer<TKey>(schemaRegistry).AsSyncOverAsync();
            }
        }

        protected override IDeserializer<TValue> ValueDeserializer
        {
            get
            {
                var schemaRegistryConfig = new SchemaRegistryConfig
                {
                    Url = _schemaRegistryConfig.Url,
                    BasicAuthCredentialsSource = AuthCredentialsSource.UserInfo,
                    BasicAuthUserInfo = _schemaRegistryConfig.BasicAuthUserInfo
                };

                var schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig);

                return new AvroDeserializer<TValue>(schemaRegistry).AsSyncOverAsync();
            }
        }

        protected override async Task<KafkaReply> HandleMessageAsync(
            KafkaConsumerMessage<TKey, TValue> messageReceivedArgs,
            CancellationToken cancellationToken)
        {
            try
            {
                using var timer = _metricService.StartTimer($"{KafkaConsumerStatName}.latency");
                await ProcessMessageAsync(messageReceivedArgs);
                return KafkaReply.Commit;
            }
            catch (Exception exception)
            {
                _metricService.Increment(KafkaConsumerStatName, new List<string> { "result:application_error" });
                Logger.LogError(exception, "{Consumer}: Failed to process the message", ConsumerName);
                return KafkaReply.Retry;
            }
        }

        protected abstract Task ProcessMessageAsync(KafkaConsumerMessage<TKey, TValue> e);
    }
}