using CRA.FactorsListener.Cdc.Configs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Questrade.Library.PubSubClientHelper.Extensions;
using Questrade.Library.PubSubClientHelper.HealthCheck;
using Questrade.Library.PubSubClientHelper.Primitives;
using Questrade.Library.PubSubClientHelper.Publisher.Simple;

namespace CRA.FactorsListener.Core.Extension
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPublisher<TMessage>(this IServiceCollection services,
            IConfiguration configuration)
            where TMessage : class, IMessageWithMetadata, new()
        {
            var configKey = $"GooglePubSub:Publishers:{typeof(TMessage).Name}";
            var publisherConfiguration = configuration.GetSection(configKey).Get<PublisherConfig<TMessage>>();

            services.RegisterSimplePublisher<
                PublisherConfig<TMessage>,
                TMessage,
                SimplePubsubPublisherService<PublisherConfig<TMessage>, TMessage>,
                PublisherHealthCheck<PublisherConfig<TMessage>, TMessage>>(publisherConfiguration);

            return services;
        }
    }
}