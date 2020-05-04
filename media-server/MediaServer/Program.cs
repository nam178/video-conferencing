﻿using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MediaServerConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            ThreadPool.SetMinThreads(32, 32);
            LogManager.LoadConfiguration(Path.Combine(Assembly.GetEntryAssembly().Location, "..", "nlog.config"));
            LogManager.GetCurrentClassLogger().Info("Welcome");
            try
            {
                await new HostBuilder()
                    .ConfigureHostConfiguration(config => config.AddEnvironmentVariables())
                    .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                    .ConfigureAppConfiguration((hostingContext, config) =>
                    {
                        config.AddEnvironmentVariables();
                    })
                    .ConfigureServices((context, services) =>
                    {
                        services.AddOptions();
                    })
                    .ConfigureContainer<ContainerBuilder>(builder =>
                    {
                        builder.RegisterModule<global::MediaServer.Api.WebSocket.IoC.WebSocketModule>();
                        builder.RegisterModule<global::MediaServer.Core.IoC.CoreModule>();
                        builder.RegisterModule<global::MediaServer.Common.IoC.CommonModule>();
                    })
                    .RunConsoleAsync();
            }
            catch(Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Fatal(ex);
                NLog.LogManager.Flush();
            }
        }
    }
}
