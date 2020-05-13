using Autofac;
using MediaServer.Api.WebSocket.Net;
using Newtonsoft.Json;
using NLog;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MediaServer.Common.Patterns;

namespace MediaServer.Api.WebSocket.CommandHandlers
{
    sealed class StringCommandHandler : IHandler<IWebSocketRemoteDevice, string>
    {
        readonly Type[] _possibleCommandTypes;
        readonly ILifetimeScope _scope;
        readonly static ILogger _logger = LogManager.GetCurrentClassLogger();

        public StringCommandHandler(ILifetimeScope scope)
        {
            _possibleCommandTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.GetInterfaces().Any(f =>
                {
                    return f.IsGenericType && f.GetGenericTypeDefinition() == typeof(IHandler<,>);
                }))
                .ToArray();
            _scope = scope
                ?? throw new ArgumentNullException(nameof(scope));
        }

        public Task HandleAsync(IWebSocketRemoteDevice device, string commandArgs)
        {
            var commandFormat = JsonConvert.DeserializeObject<CommandFormat>(commandArgs);

            var commandHandlerType = _possibleCommandTypes.FirstOrDefault(cmd =>
            {
                var argsName = GetHandleAsyncParameterType(cmd).Name;
                return string.Equals(argsName, commandFormat.Command, StringComparison.InvariantCulture);
            });

            if(null == commandHandlerType)
                throw new InvalidDataException($"Invalid command {commandFormat.Command}");

            var commandHandler = _scope.Resolve(commandHandlerType);
            // HACK
            // don't log heartbeats.. annoying.
            if(commandHandlerType.Name != "HeartBeatCommandHandler")
            {
                _logger.Trace($"Executing command {commandFormat.Command}, Args={commandFormat.Args}");
            }
            
            return (Task)GetHandleAsyncMethod(commandHandlerType)
                .Invoke(commandHandler, new[] {
                    device,
                    JsonConvert.DeserializeObject(
                        JsonConvert.SerializeObject(commandFormat.Args),
                        GetHandleAsyncParameterType(commandHandlerType))
                });
        }

        static Type GetHandleAsyncParameterType(Type commandHandlerType)
        {
            return GetHandleAsyncMethod(commandHandlerType).GetParameters()[1].ParameterType;
        }

        static MethodInfo GetHandleAsyncMethod(Type commandHandlerType)
        {
            return commandHandlerType.GetMethod(nameof(HandleAsync), BindingFlags.Instance | BindingFlags.Public);
        }
    }
}
