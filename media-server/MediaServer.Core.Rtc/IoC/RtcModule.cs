using Autofac;
using MediaServer.Core.Rtc.Decorators;
using MediaServer.Core.Rtc.Repositories;
using MediaServer.Core.Services;
using MediaServer.Core.Services.RoomManager;

namespace MediaServer.Core.Rtc.IoC
{
    public sealed class RtcModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
            builder.RegisterDecorator<DeviceDisconnectionRequestHandlerDecorator, IRemoteDeviceService<DeviceDisconnectionRequest>>();
            builder
                .RegisterType<PeerConnectionRepository>()
                .AsImplementedInterfaces()
                .SingleInstance();
        }
    }
}
