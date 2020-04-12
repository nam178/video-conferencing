using Autofac;
using MediaServer.Common.Threading;
using MediaServer.Core.Repositories;
using MediaServer.Core.Services;
using MediaServer.Core.Services.RoomManagement;
using Microsoft.Extensions.Hosting;

namespace MediaServer.Core.IoC
{
    public sealed class CoreModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            // A global dispatch queue:
            // used for dispatching
            // tasks that operates the room management, as it's not thread safe.
            builder.RegisterType<ThreadPoolDispatchQueue>().As<IDispatchQueue>().SingleInstance();

            // Repositories
            builder.RegisterType<RoomRepository>().As<IRoomRepository>().SingleInstance();

            // Handlers
            builder.RegisterType<NewRoomRequestHandler>().AsImplementedInterfaces();

            // HostedServices
            builder.RegisterType<GlobalDispatchQueueStarter>().As<IHostedService>();
        }
    }
}
