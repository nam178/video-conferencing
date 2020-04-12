using Autofac;
using MediaServer.Common.Threading;
using MediaServer.Core.Repositories;
using MediaServer.Core.Services;
using MediaServer.Core.Services.RoomManager;
using MediaServer.Core.Services.ServerManager;
using Microsoft.Extensions.Hosting;

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

            // A central parallel queue
            builder
                .RegisterType<ParallelQueue>().As<IParallelQueue>().SingleInstance()
                .OnActivated(e =>
                {
                    e.Instance.Start();
                    NLog.LogManager.GetCurrentClassLogger().Info($"Central parallel queue started;");
                });

            // Repositories
            builder.RegisterType<RoomRepository>().As<IRoomRepository>().SingleInstance();
            builder.RegisterType<RemoteDeviceUserProfileMapping>().As<IRemoteDeviceUserProfileMappings>().SingleInstance();

            // Handlers
            builder.RegisterType<NewRoomRequestHandler>().AsImplementedInterfaces();
            builder.RegisterType<JoinRoomRequestHandler>().AsImplementedInterfaces();
            builder.RegisterType<DeviceDisconnectionRequestHandler>().AsImplementedInterfaces();
            builder.RegisterType<SendStatusUpdateRequestHandler>().AsImplementedInterfaces();
        }
    }
}
