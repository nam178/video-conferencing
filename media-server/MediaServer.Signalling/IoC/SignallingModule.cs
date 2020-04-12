using Autofac;
using MediaServer.Signalling.CommandHandlers;
using MediaServer.Signalling.Handlers;
using MediaServer.Signalling.Net;

namespace MediaServer.Signalling.IoC
{
    public sealed class SignallingModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<WebSocketServer>().AsImplementedInterfaces();
            builder.RegisterType<StringCommandHandler>().AsImplementedInterfaces();
            builder.RegisterType<CreateRoomCommandHandler>().AsSelf();
        }
    }
}
