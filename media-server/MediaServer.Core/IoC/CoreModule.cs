using Autofac;
using MediaServer.Common.Threading;
using MediaServer.Core.Models;
using MediaServer.Core.Repositories;
using MediaServer.Core.Services.MediaRouting;
using MediaServer.Core.Services.PeerConnection;
using MediaServer.Core.Services.RoomManager;
using MediaServer.Core.Services.ServerManager;

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
            builder.RegisterType<DeviceDisconnectionRequestHandler>().AsImplementedInterfaces();
            builder.RegisterType<SendSyncMessageRequestHandler>().AsImplementedInterfaces();
            builder.RegisterType<RTCIceCandidateHandler>().AsImplementedInterfaces();
            builder.RegisterType<RTCSessionDescriptionHandler>().AsImplementedInterfaces();
            builder.RegisterType<SetTrackQualityRequestHandler>().AsImplementedInterfaces();

            // Factories
            builder.RegisterType<RoomFactory>().AsImplementedInterfaces();
            builder.RegisterType<WebRtcInfra>().AsSelf();
        }
    }
}
