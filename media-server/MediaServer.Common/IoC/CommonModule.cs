using Autofac;
using MediaServer.Common.Time;
using MediaServer.Common.Utils;
using NLog;
using System;

namespace MediaServer.Common.IoC
{
    public sealed class CommonModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<TimerFactory>().AsImplementedInterfaces();
            builder.RegisterType<WatchDog>().As<IWatchDog>().SingleInstance()
                .OnActivated(args =>
                {
                    LogManager.GetCurrentClassLogger().Info($"WatchDog created.");
                });
        }
    }
}
