using MediaServer.Common.Mediator;
using MediaServer.Models;
using NLog;
using System;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.RoomManager
{
    sealed class DeviceDisconnectionRequestHandler : IRemoteDeviceService<DeviceDisconnectionRequest>
    {
        readonly IHandler<SendSyncMessageRequest> _statusUpdateSender;
        readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public DeviceDisconnectionRequestHandler(IHandler<SendSyncMessageRequest> statusUpdateSender)
        {
            _statusUpdateSender = statusUpdateSender
                ?? throw new ArgumentNullException(nameof(statusUpdateSender));
        }

        public async Task HandleAsync(IRemoteDevice remoteDevice, DeviceDisconnectionRequest request)
        {
            var previousData = remoteDevice.GetCustomData();

            // Remove associated PeerConnections
            foreach(var peer in previousData.PeerConnections)
            {
                using(peer)
                {
                    peer.Close();
                }
                _logger.Debug($"PeerConnection closed due to device disconnect, device {remoteDevice}");
            }

            // Clear any data associated with this device.
            // It's officially logged out
            remoteDevice.SetCustomData(new Models.RemoteDeviceData());

            // Send status update so devices can update their UIs
            if(previousData.Room != null)
            {
                await _statusUpdateSender.HandleAsync(new SendSyncMessageRequest
                {
                    Room = previousData.Room
                });
            }
        }
    }
}
