namespace MassTransit.WebJobs.RabbitMqIntegration
{
    using RabbitMQ.Client.Events;
    using System;
    using System.Threading;
    using System.Threading.Tasks;


    /// <summary>
    /// Called by an Azure Function to handle messages using configured consumers, sagas, or activities
    /// </summary>
    public interface IMessageReceiver :
        IDisposable
    {
        /// <summary>
        /// Configure all registered consumers, sagas, and activities on the receiver and handle the message
        /// </summary>
        /// <param name="queueName">The input entity name, used for the receiver InputAddress</param>
        /// <param name="message">The RabbitMq message</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task Handle(string queueName, BasicDeliverEventArgs message, CancellationToken cancellationToken);

        /// <summary>
        /// Configure the specified consumer on the receiver and handle the message
        /// </summary>
        /// <param name="queueName">The input entity name, used for the receiver InputAddress</param>
        /// <param name="message">The RabbitMq message</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task HandleConsumer<TConsumer>(string queueName, BasicDeliverEventArgs message, CancellationToken cancellationToken)
            where TConsumer : class, IConsumer;

        /// <summary>
        /// Configure the specified saga on the receiver and handle the message
        /// </summary>
        /// <param name="queueName">The input entity name, used for the receiver InputAddress</param>
        /// <param name="message">The RabbitMq message</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task HandleSaga<TSaga>(string queueName, BasicDeliverEventArgs message, CancellationToken cancellationToken)
            where TSaga : class, ISaga;


        /// <summary>
        /// Configure the specified execute activity on the receiver and handle the message
        /// </summary>
        /// <param name="queueName">The input entity name, used for the receiver InputAddress</param>
        /// <param name="message">The RabbitMq message</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task HandleExecuteActivity<TActivity>(string queueName, BasicDeliverEventArgs message, CancellationToken cancellationToken)
            where TActivity : class;

        Task Handle(string exchangeName, string keyName, BasicDeliverEventArgs message, CancellationToken cancellationToken);

        Task HandleConsumer<TConsumer>(string exchangeName, string keyName, BasicDeliverEventArgs message, CancellationToken cancellationToken)
            where TConsumer : class, IConsumer;


        Task HandleSaga<TSaga>(string exchangeName, string keyName, BasicDeliverEventArgs message, CancellationToken cancellationToken)
            where TSaga : class, ISaga;


        Task HandleExecuteActivity<TActivity>(string exchangeName, string keyName, BasicDeliverEventArgs message, CancellationToken cancellationToken)
            where TActivity : class;
    }
}
