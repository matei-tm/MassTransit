namespace MassTransit.WebJobs.RabbitMqIntegration
{
    using GreenPipes;
    using MassTransit.Pipeline;
    using RabbitMQ.Client.Events;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IRabbitMqMessageReceiver : IReceiveObserverConnector,
        IPublishObserverConnector,
        ISendObserverConnector,
        IConsumeMessageObserverConnector,
        IConsumeObserverConnector,
        IProbeSite
    {
        Task Handle(BasicDeliverEventArgs message, CancellationToken cancellationToken, Action<ReceiveContext> contextCallback);
    }
}
