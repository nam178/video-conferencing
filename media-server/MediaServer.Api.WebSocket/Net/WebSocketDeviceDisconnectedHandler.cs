using MediaServer.Common.Patterns;
using MediaServer.Core.Services.RoomManagement;
using NLog;
using System.Threading.Tasks;

namespace MediaServer.Api.WebSocket.Net
{
    sealed class WebSocketDeviceDisconnectedHandler : IHandler<IWebSocketRemoteDevice>
    {
        readonly IDeviceDisconnector _disconnector;
        readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public WebSocketDeviceDisconnectedHandler(IDeviceDisconnector disconnector)
        {
            _disconnector = disconnector
                ?? throw new System.ArgumentNullException(nameof(disconnector));
        }

        public Task HandleAsync(IWebSocketRemoteDevice device)
        {
            // When a device disconnect,
            // we'll report that to the core. 
            return _disconnector.DisconnectDeviceAsync(device).ContinueWith(task =>
            {
                if(task.Exception != null)
                {
                    _logger.Fatal(task.Exception.InnerException,
                        "Unexpected exception thrown by core handler " +
                        "when handling disconnection request");
                }
            });
        }
    }
}
