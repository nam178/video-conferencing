﻿using Autofac;
using MediaServer.Common.Time;
using MediaServer.Common.Utils;
using NLog;

namespace MediaServer.Common.IoC
{
    public sealed class CommonModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            // singletons
            builder.RegisterType<WatchDog>().As<IWatchDog>().SingleInstance()
                .OnActivated(args =>
                {
                    LogManager.GetCurrentClassLogger().Info($"WatchDog created.");
                });


            // Transient
            builder.RegisterType<Clock>().AsImplementedInterfaces();
            builder.RegisterType<TimerFactory>().AsImplementedInterfaces();
        }
    }
}
