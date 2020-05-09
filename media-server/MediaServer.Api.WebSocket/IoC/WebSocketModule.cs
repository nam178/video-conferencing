using Autofac;
using MediaServer.Api.WebSocket.CommandHandlers;
using MediaServer.Api.WebSocket.Net;

namespace MediaServer.Api.WebSocket.IoC
{
    public sealed class WebSocketModule : Autofac.Module
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
            builder.RegisterType<AddIceCandidateHandler>().AsSelf();
            builder.RegisterType<SetOfferCommandHandler>().AsSelf();
            builder.RegisterType<AuthenticateCommandHandler>().AsSelf();
            builder.RegisterType<SetTransceiversMetadataCommandHandler>().AsSelf();
            builder.RegisterType<SetAnswerCommandHandler>().AsSelf();
        }
    }
}
