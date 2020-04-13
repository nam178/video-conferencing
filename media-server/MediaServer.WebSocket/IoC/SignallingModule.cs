using Autofac;
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
            builder.RegisterType<WebSocketDeviceConnectedHandler>().AsSelf();
            builder.RegisterType<WebSocketDeviceDisconnectedHandler>().AsSelf();
            builder.RegisterType<HttpClientDispatcher>()
                .AsImplementedInterfaces()
                .WithParameter(
                    (p, x) => p.Name == "remoteDeviceDisconenctedHandler",
                    (p, x) => x.Resolve<WebSocketDeviceDisconnectedHandler>())
                .WithParameter(
                    (p, x) => p.Name == "remoteDeviceConnectedHandler",
                    (p, x) => x.Resolve<WebSocketDeviceConnectedHandler>());

            // Command handlers
            builder.RegisterType<StringCommandHandler>().AsImplementedInterfaces();
            builder.RegisterType<JoinRoomCommandHandler>().AsSelf();
            builder.RegisterType<CreateRoomCommandHandler>().AsSelf();
            builder.RegisterType<HeartBeatCommandHandler>().AsSelf();
        }
    }
}
