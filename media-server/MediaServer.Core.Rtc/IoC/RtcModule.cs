using Autofac;
using MediaServer.Rtc.Repositories;

namespace MediaServer.Rtc.IoC
{
    public sealed class RtcModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<PeerConnectionRepository>().AsImplementedInterfaces().SingleInstance();
        }
    }
}
