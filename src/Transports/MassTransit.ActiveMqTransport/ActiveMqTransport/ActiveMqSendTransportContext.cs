namespace MassTransit.ActiveMqTransport
{
    using Apache.NMS;
    using Transports;


    public interface ActiveMqSendTransportContext :
        SendTransportContext,
        IPipeContextSource<SessionContext>
    {
        IPipe<SessionContext> ConfigureTopologyPipe { get; }

        string EntityName { get; }

        DestinationType DestinationType { get; }

        ISessionContextSupervisor SessionContextSupervisor { get; }
    }
}
