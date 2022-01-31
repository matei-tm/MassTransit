namespace MassTransit.WebJobs.RabbitMqIntegration
{
    using MassTransit.RabbitMqTransport.Configuration;
    using RabbitMQ.Client.Events;
    using Registration;
    using Saga;
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;


    public class MessageReceiver :
        IMessageReceiver
    {
        readonly IAsyncBusHandle _busHandle;
        readonly IRabbitMqHostConfiguration _hostConfiguration;
        readonly ConcurrentDictionary<string, Lazy<IRabbitMqMessageReceiver>> _receivers;
        readonly IBusRegistrationContext _registration;

        public MessageReceiver(IBusRegistrationContext registration, IAsyncBusHandle busHandle, IBusInstance busInstance)
        {
            _hostConfiguration = busInstance.HostConfiguration as IRabbitMqHostConfiguration
                ?? throw new ConfigurationException("The hostConfiguration was not properly configured for Azure Service Bus");

            _registration = registration;
            _busHandle = busHandle;

            _receivers = new ConcurrentDictionary<string, Lazy<IRabbitMqMessageReceiver>>();
        }

        public Task Handle(string queueName, BasicDeliverEventArgs message, CancellationToken cancellationToken)
        {
            var receiver = CreateMessageReceiver(queueName, cfg =>
            {
                cfg.ConfigureConsumers(_registration);
                cfg.ConfigureSagas(_registration);
            });

            return receiver.Handle(message, cancellationToken);
        }

        public Task HandleConsumer<TConsumer>(string queueName, BasicDeliverEventArgs message, CancellationToken cancellationToken)
            where TConsumer : class, IConsumer
        {
            var receiver = CreateMessageReceiver(queueName, cfg =>
            {
                cfg.ConfigureConsumer<TConsumer>(_registration);
            });

            return receiver.Handle(message, cancellationToken);
        }

        public Task HandleSaga<TSaga>(string queueName, BasicDeliverEventArgs message, CancellationToken cancellationToken)
            where TSaga : class, ISaga
        {
            var receiver = CreateMessageReceiver(queueName, cfg =>
            {
                cfg.ConfigureSaga<TSaga>(_registration);
            });

            return receiver.Handle(message, cancellationToken);
        }


        public Task HandleExecuteActivity<TActivity>(string queueName, BasicDeliverEventArgs message, CancellationToken cancellationToken)
            where TActivity : class
        {
            var receiver = CreateMessageReceiver(queueName, cfg =>
            {
                cfg.ConfigureExecuteActivity(_registration, typeof(TActivity));
            });

            return receiver.Handle(message, cancellationToken);
        }

        public void Dispose()
        {
        }

        IRabbitMqMessageReceiver CreateMessageReceiver(string queueName, Action<IReceiveEndpointConfigurator> configure)
        {
            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentNullException(nameof(queueName));
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            return _receivers.GetOrAdd(queueName, name => new Lazy<IRabbitMqMessageReceiver>(() =>
            {
                var endpointConfiguration = _hostConfiguration.CreateReceiveEndpointConfiguration(queueName);

                var configurator = new RabbitMqMessageReceiverConfiguration(_hostConfiguration, endpointConfiguration);

                configure(configurator);

                return configurator.Build();
            })).Value;
        }

        
    }
}
