using Autofac;
using MediaServer.Common.Threading;
using MediaServer.Core.Adapters;
using MediaServer.Core.Models;
using MediaServer.Core.Repositories;
using MediaServer.Core.Services.Negotiation;
using MediaServer.Core.Services.RoomManagement;

namespace MediaServer.Core.IoC
{
    public sealed class CoreModule : Autofac.Module
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

            // Repositories
            builder.RegisterType<RoomRepository>().As<IRoomRepository>().SingleInstance();

            // Handlers
            builder.RegisterType<NewRoomRequestHandler>().AsImplementedInterfaces();
            builder.RegisterType<JoinRoomRequestHandler>().AsImplementedInterfaces();
            builder.RegisterType<DeviceDisconnector>().AsImplementedInterfaces();
            builder.RegisterType<SyncMessenger>().AsImplementedInterfaces();
            builder.RegisterType<IceCandidateHandler>().AsImplementedInterfaces();
            builder.RegisterType<OfferHandler>().AsImplementedInterfaces();
            builder.RegisterType<TransceiverMetadataSetter>().AsImplementedInterfaces();
            builder.RegisterType<AnswerHandler>().AsImplementedInterfaces();

            // Factories
            builder.RegisterType<RoomFactory>().AsImplementedInterfaces();
            builder.RegisterType<WebRtcInfraAdapter>().AsSelf();

            // Negotiation service
            builder.RegisterType<OfferMessageSubscriber>().AsImplementedInterfaces();
            builder.RegisterType<RenegotiationMessageSubscriber>().AsImplementedInterfaces();
        }
    }
}
