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
        /// <summary>
        /// Handles the <paramref name="message"/>
        /// </summary>
        /// <param name="message">the RabbitMq message</param>
        /// <param name="cancellationToken">Specify an optional cancellationToken</param>
        /// <param name="contextCallback">Callback to adjust the context</param>
        /// <returns></returns>
        Task Handle(BasicDeliverEventArgs message, CancellationToken cancellationToken = default, Action<ReceiveContext> contextCallback = null);
    }
}
