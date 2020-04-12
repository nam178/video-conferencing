using MediaServer.Models;
using MediaServer.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;
using System.Threading.Tasks;

namespace MediaServer.Signalling.Net
{
    sealed class WebSocketClientRemoteDevice : IRemoteDevice
    {
        readonly WebSocketClient _client;
        readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public WebSocketClientRemoteDevice(WebSocketClient client)
        {
            _client = client ?? throw new System.ArgumentNullException(nameof(client));
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
            _logger.Debug($"Sending message to {_client}, {message}");
            return _client.SendAsync(message);
        }

        public override string ToString()
        {
            return $"[WebSocketClientRemoteDevice {_client}]";
        }
    }
}
