namespace MassTransit.WebJobs.RabbitMqIntegration
{
    using MassTransit.Configuration;
    using MassTransit.Configurators;
    using MassTransit.RabbitMqTransport.Builders;
    using MassTransit.RabbitMqTransport.Configuration;
    using System;


    public class RabbitMqMessageReceiverConfiguration : ReceiverConfiguration
    {
        private IRabbitMqHostConfiguration _hostConfiguration;
        private IRabbitMqReceiveEndpointConfiguration _endpointConfiguration;

        public RabbitMqMessageReceiverConfiguration(IRabbitMqHostConfiguration hostConfiguration, IRabbitMqReceiveEndpointConfiguration endpointConfiguration) : base(endpointConfiguration)
        {
            _hostConfiguration = hostConfiguration;
            _endpointConfiguration = endpointConfiguration;
        }

        public IRabbitMqMessageReceiver Build()
        {
            var result = BusConfigurationResult.CompileResults(Validate());

            try
            {
                var builder = new RabbitMqReceiveEndpointBuilder(_hostConfiguration, _endpointConfiguration);

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
