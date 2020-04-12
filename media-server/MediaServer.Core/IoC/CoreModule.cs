using Autofac;
using MediaServer.Models;

namespace MediaServer.Core.IoC
{
    public sealed class CoreModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<Room>().AsSelf().SingleInstance();
        }
    }
}
