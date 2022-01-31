using MassTransit.Configuration;
using MassTransit.Configurators;
using MassTransit.RabbitMqTransport.Builders;
using MassTransit.RabbitMqTransport.Configuration;
using RabbitMQ.Client;
using System;

namespace MassTransit.WebJobs.RabbitMqIntegration
{
    public class RabbitMqMessageReceiverConfiguration : ReceiverConfiguration
    {
        private IRabbitMqHostConfiguration hostConfiguration;
        private IRabbitMqReceiveEndpointConfiguration endpointConfiguration;

        public RabbitMqMessageReceiverConfiguration(IRabbitMqHostConfiguration hostConfiguration, IRabbitMqReceiveEndpointConfiguration endpointConfiguration) : base(endpointConfiguration)
        {
            this.hostConfiguration = hostConfiguration;
            this.endpointConfiguration = endpointConfiguration;
        }

        public IRabbitMqMessageReceiver Build()
        {
            var result = BusConfigurationResult.CompileResults(Validate());

            try
            {
                var builder = new RabbitMqReceiveEndpointBuilder(hostConfiguration, endpointConfiguration);

                foreach (var specification in Specifications)
                {
                    specification.Configure(builder);
                }

                return new RabbitMqMessageReceiver(builder.CreateReceiveEndpointContext());
            }
            catch (Exception ex)
            {
                throw new ConfigurationException(result, "An exception occurred creating the EventDataReceiver", ex);
            }
        }
    }
}
