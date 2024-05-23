using System;
using CRA.FactorsListener.DAL.Clients;
using CRA.FactorsListener.DAL.Configs;
using CRA.FactorsListener.DAL.HealthChecks;
using CRA.FactorsListener.DAL.Repositories;
using CRA.FactorsListener.Domain.Repositories;
using Google.Cloud.Firestore;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CRA.FactorsListener.DAL.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddFirestore(this IServiceCollection services, IConfiguration configuration)
        {
            var config = configuration.GetSection(nameof(FirestoreConfig)).Get<FirestoreConfig>();

            if (string.IsNullOrWhiteSpace(config.ProjectId))
            {
                throw new ArgumentNullException(nameof(config.ProjectId));
            }

            services.AddSingleton(_ =>
            {
                var builder = new FirestoreDbBuilder
                {
                    ProjectId = config.ProjectId
                };

                if (string.IsNullOrWhiteSpace(config.EmulatorHost))
                {
                    return builder.Build();
                }

                builder.ChannelCredentials = ChannelCredentials.Insecure;
                builder.Endpoint = config.EmulatorHost;

                return builder.Build();
            });
            services.AddHealthChecks().AddCheck<FirestoreHealthCheck>("firestore-health-check");
        }

        public static void AddRepositories(this IServiceCollection services)
        {
            services.TryAddSingleton(typeof(FirestoreClientFactory<>));
            services.TryAddSingleton<IUserRepository, UserRepository>();
            services.TryAddSingleton<IPersonRepository, PersonRepository>();
            services.TryAddSingleton<IPersonAddressRepository, PersonAddressRepository>();
            services.TryAddSingleton<IUserAccountRepository, UserAccountRepository>();
        }
    }
}
