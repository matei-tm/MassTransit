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

        public ConnectHandle ConnectReceiveObserver(IReceiveObserver observer)
        {
            return _context.ConnectReceiveObserver(observer);
        }

        public ConnectHandle ConnectPublishObserver(IPublishObserver observer)
        {
            return _context.ConnectPublishObserver(observer);
        }

        public ConnectHandle ConnectSendObserver(ISendObserver observer)
        {
            return _context.ConnectSendObserver(observer);
        }

        public async Task Handle(BasicDeliverEventArgs message, CancellationToken cancellationToken, Action<ReceiveContext> contextCallback)
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

            CancellationTokenRegistration registration = default;
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

        public ConnectHandle ConnectConsumeMessageObserver<T>(IConsumeMessageObserver<T> observer) where T : class
        {
            return _context.ReceivePipe.ConnectConsumeMessageObserver(observer);
        }

        public ConnectHandle ConnectConsumeObserver(IConsumeObserver observer)
        {
            return _context.ReceivePipe.ConnectConsumeObserver(observer);
        }
    }
}
