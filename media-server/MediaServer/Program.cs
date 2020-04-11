using Autofac;
using Autofac.Extensions.DependencyInjection;
using MediaServer.Models;
using MediaServer.WebRtc;
using MediaServer.WebRtc.Managed;
using MediaServer.WebSocket;
using MediaServer.WebSocket.CommandHandlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace MediaServerConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var iceServerConfig = new PeerConnectionInterop2.IceServerConfig[]
            {
                new PeerConnectionInterop2.IceServerConfig {
                    CommaSeperatedUrls = "BarBoo1;BarBoo2",
                    Username = "boo",
                    Password = "bar"
                },
                new PeerConnectionInterop2.IceServerConfig {
                    Username = null,
                    Password = "bar"
                },
            };

            try
            {
                Console.WriteLine(PeerConnectionInterop2.Create(iceServerConfig, iceServerConfig.Length));
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
            
            return;

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
                        builder.RegisterType<WebSocketServer>().AsImplementedInterfaces();
                        builder.RegisterType<StringCommandHandler>().AsImplementedInterfaces();
                        builder.RegisterType<HeartBeatCommandHandler>().AsSelf().SingleInstance();
                        builder.RegisterType<RegisterCommandHandler>().AsSelf().SingleInstance();
                        builder.RegisterType<SetOfferCommandHandler>().AsSelf().SingleInstance();
                        builder.RegisterType<AddIceCandidateHandler>().AsSelf().SingleInstance();

                        builder.RegisterType<Room>().AsSelf().SingleInstance();

                        builder.RegisterType<PeerManager>().As<IHostedService>();
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
