using Autofac;
using MediaServer.Common.Threading;
using MediaServer.Core.Services.Negotiation.Handlers;
using MediaServer.Core.Services.Negotiation.MessageQueue;
using MediaServer.Core.Services.RoomManagement;

namespace MediaServer.Core.Services.IoC
{
    public sealed class CoreModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            // A central dispatch queue:
            // used for dispatching
            // tasks that operates the room management, as it's not thread safe.
            builder
                .RegisterType<ThreadPoolDispatchQueue>().As<IDispatchQueue>().SingleInstance()
                .OnActivated(e =>
                {
                    e.Instance.Start();
                    NLog.LogManager.GetCurrentClassLogger().Info($"Central dispatch/serial queue started;");
                });

            // Handlers
            builder.RegisterType<NewRoomRequestHandler>().AsImplementedInterfaces();
            builder.RegisterType<JoinRoomRequestHandler>().AsImplementedInterfaces();
            builder.RegisterType<DeviceDisconnector>().AsImplementedInterfaces();
            builder.RegisterType<SyncMessenger>().AsImplementedInterfaces();
            builder.RegisterType<IceCandidateHandler>().AsImplementedInterfaces();
            builder.RegisterType<OfferHandler>().AsImplementedInterfaces();
            builder.RegisterType<AnswerHandler>().AsImplementedInterfaces();
            builder.RegisterType<AckTransceiverMetadataHandler>().AsImplementedInterfaces();

            // Negotiation service
            builder.RegisterType<SdpOfferMessageSubscriber>().AsImplementedInterfaces();
            builder.RegisterType<SdpAnswerMessageSubscriber>().AsImplementedInterfaces();
            builder.RegisterType<RenegotiationMessageSubscriber>().AsImplementedInterfaces();
            builder.RegisterType<IceCandidateMessageSubscriber>().AsImplementedInterfaces();
            builder.RegisterType<RemoteTransceiverMetadataMessageSubscriber>().AsImplementedInterfaces();
            builder.RegisterType<NegotiationService>().AsImplementedInterfaces().SingleInstance();

            // Observers
            builder.RegisterType<TransceiverMetadataUpdatedEventObserver>().AsImplementedInterfaces();
        }
    }
}
