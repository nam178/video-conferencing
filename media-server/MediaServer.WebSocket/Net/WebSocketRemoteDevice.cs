using MediaServer.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;
using System;
using System.Threading.Tasks;

namespace MediaServer.WebSocket.Net
{
    /// <summary>
    /// Implementation of a "remote device" based on web socket connections.
    /// That is, each web socket connection is considered a remote device
    /// </summary>
    sealed class WebSocketRemoteDevice : IWebSocketRemoteDevice
    {
        static readonly ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        public WebSocketClient WebSocketClient { get; }

        public WebSocketRemoteDevice(WebSocketClient client)
        {
            WebSocketClient = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task SendAsync(string command, object args)
        {
            await WebSocketClient.SendAsync(Serialize(command, args));
            _logger.Debug($"Command {command}, Args={JsonConvert.SerializeObject(args)} sent to device {this}");
        }

        public override string ToString() => $"[WebSocketClientRemoteDevice {WebSocketClient}]";

        public Task SendUserUpdateAsync(RemoteDeviceUserUpdateMessage message) => SendAsync("UpdateUsers", message);

        public void Teminate() => WebSocketClient.Dispose();

        void IDisposable.Dispose() => Teminate();

        static string Serialize(string command, object args)
        {
            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            var message = JsonConvert.SerializeObject(new CommandFormat
            {
                Command = command,
                Args = args
            }, serializerSettings);
            return message;
        }
    }
}
