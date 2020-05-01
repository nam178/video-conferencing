using MediaServer.Common.Mediator;
using MediaServer.Core.Services.RoomManager;
using MediaServer.Models;
using NLog;
using System.Threading.Tasks;

namespace MediaServer.WebSocket.Net
{
    sealed class WebSocketDeviceDisconnectedHandler : IHandler<IWebSocketRemoteDevice>
    {
        readonly IHandler<IRemoteDevice, DeviceDisconnectionRequest> _coreHandler;
        readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public WebSocketDeviceDisconnectedHandler(IHandler<IRemoteDevice, DeviceDisconnectionRequest> coreHandler)
        {
            _coreHandler = coreHandler
                ?? throw new System.ArgumentNullException(nameof(coreHandler));
        }

        public Task HandleAsync(IWebSocketRemoteDevice device)
        {
            // When a device disconnect,
            // we'll report that to the core. 
            return _coreHandler.HandleAsync(device, new DeviceDisconnectionRequest()).ContinueWith(task =>
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
