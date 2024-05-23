using System;
using CRA.FactorsListener.Core.Extension;
using CRA.FactorsListener.DAL.Extensions;
using CRA.FactorsListener.Cdc.Consumers;
using CRA.FactorsListener.Cdc.Extensions;
using CRA.FactorsListener.Cdc.Models.Events;
using CRA.FactorsListener.Cdc.Services.Metrics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Questrade.Library.HealthCheck.AspNetCore.Extensions;

namespace CRA.FactorsListener.Core
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddQuestradeHealthCheck(_configuration);
            
            services.AddFirestore(_configuration);
            services.AddRepositories();

            services.AddKafka(_configuration)
                .AddKafkaConsumer<PersonConsumer>()
                .AddKafkaConsumer<PersonAddressConsumer>()
                .AddKafkaConsumer<UserConsumer>()
                .AddKafkaConsumer<UserAccountConsumer>()
                .AddKafkaConsumer<AccountConsumer>()
                .AddKafkaConsumer<InternationalAddressConsumer>()
                .AddKafkaConsumer<DomesticAddressConsumer>()
                .AddKafkaConsumer<PoliticallyExposedPersonConsumer>()
                .AddKafkaConsumer<PersonEmploymentConsumer>();

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services
                .AddAllowListServices(_configuration)
                .AddRiskFactorsServices(_configuration)
                .AddCrmServices(_configuration);
            
            services.AddSingleton<IMetricService, MetricService>();

            services.AddPublisher<AccountChanged>(_configuration);
            services.AddPublisher<CountryChanged>(_configuration);
            services.AddPublisher<PersonEmploymentChanged>(_configuration);
            services.AddPublisher<PoliticallyExposedPersonChanged>(_configuration);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseQuestradeHealthCheck();
        }
    }
}