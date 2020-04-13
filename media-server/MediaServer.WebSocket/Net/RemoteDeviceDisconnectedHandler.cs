using MediaServer.Common.Mediator;
using MediaServer.Core.Services;
using MediaServer.Core.Services.RoomManager;
using NLog;
using System.Threading.Tasks;

namespace MediaServer.WebSocket.Net
{
    sealed class RemoteDeviceDisconnectedHandler : IHandler<IRemoteDeviceInternal>
    {
        readonly IRemoteDeviceRequestHandler<DeviceDisconnectionRequest> _coreHandler;
        readonly ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        public RemoteDeviceDisconnectedHandler(IRemoteDeviceRequestHandler<DeviceDisconnectionRequest> coreHandler)
        {
            _coreHandler = coreHandler 
                ?? throw new System.ArgumentNullException(nameof(coreHandler));
        }

        public Task HandleAsync(IRemoteDeviceInternal device)
        {
            // When a device disconnect,
            // we'll report that to the core. 
            return _coreHandler.HandleAsync(device, new DeviceDisconnectionRequest()).ContinueWith(task =>
            {
                if(task.Exception != null)
                {
                    _logger.Fatal(
                        "Unexpected exception thrown by core handler " +
                        "when handling disconnection request", task);
                }
            });
        }
    }
}
