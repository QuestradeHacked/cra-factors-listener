using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Questrade.Library.PubSubClientHelper.Primitives;

namespace CRA.FactorsListener.Cdc.Configs
{
    public class PublisherConfig<TMessage> : IPublisherConfiguration<TMessage>
        where TMessage : class, IMessageWithMetadata, new()
    {
        public PublisherConfig()
        {
        }

        public bool Enable { get; set; }

        public bool UseEmulator { get; set; }

        public string ProjectId { get; set; }

        public string TopicId { get; set; }

        public int? PublisherClientCount { get; set; }

        public string Endpoint { get; set; }

        public long ElementCountThreshold { get; set; }

        public long ByteCountThreshold { get; set; }

        public TimeSpan DelayThreshold { get; set; }

        public bool ShouldForwardErrors { get; set; }

        public bool EnableMessageOrdering { get; set; }

        public TimeSpan PublisherServiceDelay { get; set; }

        public int PublishFailureThresholdToConsiderDegraded { get; set; }

        public int PublishFailureThresholdToConsiderUnhealthy { get; set; }

        public HealthStatus DegradedStateType { get; set; }

        public HealthStatus UnhealthyStateType { get; set; }

        public TimeSpan DegradedStateDuration { get; set; }

        public TimeSpan UnhealthyStateDuration { get; set; }

        public bool ShouldRestartPublisherWhenUnhealthy { get; set; }

        public int? MaximumMessagePublishAttempts { get; set; }

        public string UniqueIdentifier { get; set; }

        public bool ShowPii { get; set; }

        public string DataContentType { get; set; }

        public bool CreateTopicWhenUsingEmulator { get; set; }

        public Task HandleMessageLogAsync(
            ILogger logger,
            LogLevel logLevel,
            TMessage message,
            string logMessage,
            CancellationToken cancellationToken = new())
        {
            logger.Log(logLevel, "{UniqueIdentifier}:{TopicId} -> {LogMessage}", UniqueIdentifier, TopicId, logMessage);
            return Task.CompletedTask;
        }
    }
}
