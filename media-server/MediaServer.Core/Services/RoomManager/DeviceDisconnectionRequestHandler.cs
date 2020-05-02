using MediaServer.Common.Mediator;
using MediaServer.Models;
using NLog;
using System;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.RoomManager
{
    sealed class DeviceDisconnectionRequestHandler : IHandler<IRemoteDevice, DeviceDisconnectionRequest>
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
            var deviceData = remoteDevice.GetCustomData();

            // Remove associated PeerConnections
            foreach(var peer in deviceData.PeerConnections)
            {
                using(peer)
                {
                    await peer.CloseAsync();
                    _logger.Debug($"{peer} closed due to device disconnect, device {remoteDevice}");
                }
            }

            // If this devie has joined a room,
            // It also need to be removed from the router, so media no longer to/from to it.
            if(deviceData.Room != null)
            {
                await deviceData.Room.VideoRouter.RemoveVideoClientAsync(remoteDevice.Id);
            }

            // If this device associated with an user,
            // Move the association.
            if(deviceData.User != null)
            {
                await deviceData.User.Room.SignallingThread.ExecuteAsync(delegate
                {
                    deviceData.User.Devices.Remove(remoteDevice);
                });
            }

            // Clear any data associated with this device.
            // It's officially logged out
            remoteDevice.SetCustomData(new Models.RemoteDeviceData());

            // Send status update so devices can update their UIs
            if(deviceData.Room != null)
            {
                await _statusUpdateSender.HandleAsync(new SendSyncMessageRequest
                {
                    Room = deviceData.Room
                });
            }
        }
    }
}
