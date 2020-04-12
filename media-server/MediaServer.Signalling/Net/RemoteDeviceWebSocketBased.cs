using MediaServer.Models;
using MediaServer.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;
using System.Threading.Tasks;

namespace MediaServer.Signalling.Net
{
    /// <summary>
    /// Implementation of a "remote device" based on web socket connections.
    /// That is, each web socket connection is considered a remote device
    /// </summary>
    sealed class RemoteDeviceWebSocketBased : IRemoteDevice
    {
        readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        internal WebSocketClient WebSocketClient { get; }

        public RemoteDeviceWebSocketBased(WebSocketClient client)
        {
            WebSocketClient = client ?? throw new System.ArgumentNullException(nameof(client));
        }

        public Task SendAsync(string command, object args)
        {
            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            var message = JsonConvert.SerializeObject(new CommandFormat
            {
                Command = command,
                Args = args
            }, serializerSettings);
            _logger.Debug($"Sending message to {WebSocketClient}, {message}");
            return WebSocketClient.SendAsync(message);
        }

        public override string ToString()
        {
            return $"[WebSocketClientRemoteDevice {WebSocketClient}]";
        }
    }
}
