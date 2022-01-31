namespace MassTransit
{
    using ExtensionsDependencyInjectionIntegration;
    using MassTransit.RabbitMqTransport;
    using MassTransit.WebJobs.RabbitMqIntegration;
    using Microsoft.ApplicationInsights.DependencyCollector;
    using Microsoft.Azure.WebJobs.Extensions.RabbitMQ;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using System;


    public static class AzureFunctionsBusConfigurationExtensions
    {
        /// <summary>
        /// Add the Azure Function support for MassTransit, which uses RabbitMq, and configures
        /// <see cref="IMessageReceiver" /> for use by functions to handle messages.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configure">
        /// Configure via <see cref="DependencyInjectionRegistrationExtensions.AddMassTransit" />, to configure consumers, etc.
        /// </param>
        /// <param name="connectionStringConfigurationKey">
        /// Optional, the name of the configuration value for the connection string.
        /// </param>
        /// <param name="configureBus">Optional, the configuration callback for the bus factory</param>
        /// <returns></returns>
        public static IServiceCollection AddMassTransitForAzureFunctions(
            this IServiceCollection services,
            Action<IServiceCollectionBusConfigurator> configure,
            string connectionStringConfigurationKey = "RabbitMQ",
            Action<IBusRegistrationContext, IRabbitMqBusFactoryConfigurator> configureBus = default)
        {
            ConfigureApplicationInsights(services);

            services
                .AddSingleton<IMessageReceiver, MessageReceiver>()
                .AddSingleton<IAsyncBusHandle, AsyncBusHandle>()
                .AddMassTransit(x =>
                {
                    configure?.Invoke(x);

                    x.UsingRabbitMq((context, cfg) =>
                    {
                        var options = context.GetRequiredService<IOptions<RabbitMQOptions>>();

                        var config = context.GetRequiredService<IConfiguration>();
                        var connectionString = config[connectionStringConfigurationKey];

                        if (string.IsNullOrWhiteSpace(connectionString))
                        {
                            throw new ArgumentNullException(connectionStringConfigurationKey, "A connection string must be used for Azure Functions.");
                        }

                        cfg.UseDelayedMessageScheduler();

                        configureBus?.Invoke(context, cfg);
                    });
                });

            return services;
        }

        static void ConfigureApplicationInsights(IServiceCollection services)
        {
            services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((module, o) =>
            {
                module.IncludeDiagnosticSourceActivities.Add("MassTransit");
            });
        }
    }
}
