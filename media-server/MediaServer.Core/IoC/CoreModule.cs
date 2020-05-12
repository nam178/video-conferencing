using Autofac;
using MediaServer.Common.Threading;
using MediaServer.Core.Adapters;
using MediaServer.Core.Models;
using MediaServer.Core.Repositories;
using MediaServer.Core.Services.Negotiation.Handlers;
using MediaServer.Core.Services.Negotiation.MessageQueue;
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
            builder.RegisterType<AnswerHandler>().AsImplementedInterfaces();

            // Factories
            builder.RegisterType<RoomFactory>().AsImplementedInterfaces();
            builder.RegisterType<WebRtcInfraAdapter>().AsSelf();

            // Negotiation service
            builder.RegisterType<SdpOfferMessageSubscriber>().AsImplementedInterfaces();
            builder.RegisterType<SdpAnswerMessageSubscriber>().AsImplementedInterfaces();
            builder.RegisterType<RenegotiationMessageSubscriber>().AsImplementedInterfaces();
            builder.RegisterType<IceCandidateMessageSubscriber>().AsImplementedInterfaces();
        }
    }
}
