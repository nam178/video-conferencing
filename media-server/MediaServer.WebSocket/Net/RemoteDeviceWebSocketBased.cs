using MediaServer.Models;
using MediaServer.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MediaServer.WebSocket.Net
{
    /// <summary>
    /// Implementation of a "remote device" based on web socket connections.
    /// That is, each web socket connection is considered a remote device
    /// </summary>
    sealed class RemoteDeviceWebSocketBased : IRemoteDeviceInternal
    {
        readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public WebSocketClient WebSocketClient { get; }

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

        public Task SendUserUpdateAsync(RemoteDeviceUserUpdateMessage message) => SendAsync("UpdateUsers", message);

        public void Teminate()
        {
            try
            {
                using(WebSocketClient.WebSocketContext.WebSocket)
                {
                    WebSocketClient.WebSocketContext.WebSocket
                        .CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "Server deliberately close the connection", CancellationToken.None)
                        .Wait(TimeSpan.FromSeconds(5));
                }
            }
            catch(Exception ex)
            {
                _logger.Warn(ex, "Exception occured when closing WebSocket");
            }
        }

        void IDisposable.Dispose() => Teminate();
    }
}
