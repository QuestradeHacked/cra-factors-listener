using System;
using System.IO.Abstractions;
using System.Net.Http.Headers;
using Confluent.SchemaRegistry;
using CRA.FactorsListener.Cdc.Configs;
using CRA.FactorsListener.Cdc.Models;
using CRA.FactorsListener.Cdc.Models.Accounts;
using CRA.FactorsListener.Cdc.Options;
using CRA.FactorsListener.Cdc.Providers;
using CRA.FactorsListener.Cdc.Services.EventHandlers;
using CRA.FactorsListener.Cdc.Services.UserMapping;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace CRA.FactorsListener.Cdc.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddKafka(this IServiceCollection services, IConfiguration configuration)
        {
            var schemaRegistryCfg = new SchemaRegistryConfig();
            configuration.GetSection("Confluent:SchemaRegistryConfig").Bind(schemaRegistryCfg);

            services.TryAddSingleton(schemaRegistryCfg);

            services.AddKafka();

            return services;
        }

        public static IServiceCollection AddRiskFactorsServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<ICdcEventHandler<Account>, CdcAccountEventHandler>();
            services.AddSingleton<ICdcEventHandler<DomesticAddress>, CdcDomesticAddressEventHandler>();
            services.AddSingleton<ICdcEventHandler<InternationalAddress>, CdcInternationalAddressEventHandler>();
            services.AddSingleton<ICdcEventHandler<PersonEmployment>, CdcPersonEmploymentEventHandler>();
            services.AddSingleton<ICdcEventHandler<PoliticallyExposedPerson>, CdcPoliticallyExposedPersonEventHandler>();

            var riskFactorsOptions = new RiskFactorsOptions();
            configuration.GetSection(nameof(RiskFactorsOptions)).Bind(riskFactorsOptions);
            services.AddSingleton(riskFactorsOptions);

            services.AddSingleton<IUserMappingsService, UserMappingsService>();
            services.AddSingleton<IPersonAddressService, PersonAddressService>();
            services.AddSingleton<IUserAccountService, UserAccountService>();
            services.AddSingleton<IUserService, UserService>();

            return services;
        }

        public static IServiceCollection AddAllowListServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IFileSystem, FileSystem>();

            return services;
        }

        public static IServiceCollection AddCrmServices(this IServiceCollection services, IConfiguration configuration)
        {
            var crmOptions = new CustomerMasterDataProviderConfig();
            configuration.GetSection(nameof(CustomerMasterDataProviderConfig)).Bind(crmOptions);
            services.AddSingleton(crmOptions);

            services.AddSingleton<ICustomerMasterDataProvider, CustomerMasterDataProvider>();
            services.AddSingleton<IGraphQLClient, GraphQLHttpClient>(provider =>
            {
                var environment = provider.GetService<IHostEnvironment>();

                var gqlHttpClientOptions = new GraphQLHttpClientOptions
                {
                    EndPoint = crmOptions.Endpoint
                };

                var graphQlClient = new GraphQLHttpClient(gqlHttpClientOptions, new NewtonsoftJsonSerializer());

                if (crmOptions.Token == null)
                {
                    if (environment.IsProd() || environment.IsUat())
                        throw new Exception("Authorization Token is required to target production.");

                    return graphQlClient;
                }

                graphQlClient.HttpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", crmOptions.Token);

                return graphQlClient;
            });

            return services;
        }
    }
}
