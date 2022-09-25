namespace MassTransit.WebJobs.RabbitMqIntegration
{
    using MassTransit.RabbitMqTransport;
    using MassTransit.Transports;
    using RabbitMQ.Client.Events;
    using System;
    using System.Threading;
    using System.Threading.Tasks;


    public class RabbitMqMessageReceiver : IRabbitMqMessageReceiver
    {
        readonly RabbitMqReceiveEndpointContext _context;
        readonly IReceivePipeDispatcher _dispatcher;

        public RabbitMqMessageReceiver(RabbitMqReceiveEndpointContext rabbitMqReceiveEndpointContext)
        {
            _context = rabbitMqReceiveEndpointContext;
            _dispatcher = rabbitMqReceiveEndpointContext.CreateReceivePipeDispatcher();
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            var scope = context.CreateScope("receiver");
            scope.Add("type", "rabbitMqMessage");
        }

        ConnectHandle IReceiveObserverConnector.ConnectReceiveObserver(IReceiveObserver observer)
        {
            return _context.ConnectReceiveObserver(observer);
        }

        ConnectHandle IPublishObserverConnector.ConnectPublishObserver(IPublishObserver observer)
        {
            return _context.ConnectPublishObserver(observer);
        }

        ConnectHandle ISendObserverConnector.ConnectSendObserver(ISendObserver observer)
        {
            return _context.ConnectSendObserver(observer);
        }

        async Task IRabbitMqMessageReceiver.Handle(BasicDeliverEventArgs message, CancellationToken cancellationToken, Action<ReceiveContext> contextCallback)
        {
            var context = new RabbitMqReceiveContext(
                exchange: message.Exchange,
                routingKey: message.RoutingKey,
                consumerTag: message.ConsumerTag,
                deliveryTag: message.DeliveryTag,
                body: message.Body.ToArray(),
                redelivered: message.Redelivered,
                properties: message.BasicProperties,
                receiveEndpointContext: _context);

            contextCallback?.Invoke(context);

            CancellationTokenRegistration registration;
            if (cancellationToken.CanBeCanceled)
            {
                registration = cancellationToken.Register(context.Cancel);
            }

            try
            {
                await _dispatcher.Dispatch(context).ConfigureAwait(false);
            }
            finally
            {
                registration.Dispose();
                context.Dispose();
            }
        }

        ConnectHandle IConsumeMessageObserverConnector.ConnectConsumeMessageObserver<T>(IConsumeMessageObserver<T> observer)
        {
            return _context.ReceivePipe.ConnectConsumeMessageObserver(observer);
        }

        ConnectHandle IConsumeObserverConnector.ConnectConsumeObserver(IConsumeObserver observer)
        {
            return _context.ReceivePipe.ConnectConsumeObserver(observer);
        }
    }
}
