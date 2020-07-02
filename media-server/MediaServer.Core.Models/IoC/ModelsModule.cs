using Autofac;
using MediaServer.Core.Models.Adapters;
using MediaServer.Core.Models.Repositories;

namespace MediaServer.Core.Models.IoC
{
    public sealed class ModelsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            // Repositories
            builder.RegisterType<RoomRepository>().As<IRoomRepository>().SingleInstance();

            // Factories
            builder.RegisterType<RoomFactory>().AsImplementedInterfaces();
            builder.RegisterType<WebRtcInfraAdapter>().AsSelf();
        }
    }
}
