using Autofac;
using MediaServer.Core.Services;
using MediaServer.Core.Services.RoomManager;
using MediaServer.Rtc.Decorators;
using MediaServer.Rtc.Repositories;

namespace MediaServer.Rtc.IoC
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
