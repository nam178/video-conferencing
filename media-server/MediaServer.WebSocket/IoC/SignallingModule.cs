using Autofac;
using MediaServer.Common.Mediator;
using MediaServer.WebSocket.CommandHandlers;
using MediaServer.WebSocket.Net;

namespace MediaServer.WebSocket.IoC
{
    public sealed class SignallingModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            // Networking stuff
            builder.RegisterType<WebSocketServer>().AsImplementedInterfaces();
            builder.RegisterType<RemoteDeviceConnectedHandler>().AsSelf();
            builder.RegisterType<RemoteDeviceDisconnectedHandler>().AsSelf();
            builder.RegisterType<HttpClientDispatcher>()
                .AsImplementedInterfaces()
                .WithParameter(
                    (p, x) => p.Name == "remoteDeviceDisconenctedHandler",
                    (p, x) => x.Resolve<RemoteDeviceDisconnectedHandler>())
                .WithParameter(
                    (p, x) => p.Name == "remoteDeviceConnectedHandler",
                    (p, x) => x.Resolve<RemoteDeviceConnectedHandler>());

            // Command handlers
            builder.RegisterType<StringCommandHandler>().AsImplementedInterfaces();
            builder.RegisterType<JoinRoomCommandHandler>().AsSelf();
            builder.RegisterType<CreateRoomCommandHandler>().AsSelf();
        }
    }
}
