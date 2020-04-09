using MediaServer.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;
using System.Threading.Tasks;

namespace MediaServer.WebSocket
{
    sealed class WebSocketSignaller : ISignaller
    {
        readonly WebSocketClient _client;
        readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public WebSocketSignaller(WebSocketClient client)
        {
            _client = client ?? throw new System.ArgumentNullException(nameof(client));
        }

        public Task SendAsync(string command, object args) {
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
    }
}
