namespace MassTransit.WebJobs.RabbitMqIntegration
{
    using MassTransit.Configuration;
    using MassTransit.RabbitMqTransport.Configuration;
    using System;
    using System.Collections.Generic;

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
            IReadOnlyList<ValidationResult> result = Validate().ThrowIfContainsFailure($"{GetType().Name} configuration is invalid:");

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
                throw new ConfigurationException(result, "An exception occurred creating the RabbitMqMessageReceiver", ex);
            }
        }
    }
}
