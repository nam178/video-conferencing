using Autofac;
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
            builder.RegisterType<HeartBeatCommandHandler>().AsSelf().SingleInstance();
            builder.RegisterType<RegisterCommandHandler>().AsSelf().SingleInstance();
            builder.RegisterType<SetOfferCommandHandler>().AsSelf().SingleInstance();
            builder.RegisterType<AddIceCandidateHandler>().AsSelf().SingleInstance();
        }
    }
}
