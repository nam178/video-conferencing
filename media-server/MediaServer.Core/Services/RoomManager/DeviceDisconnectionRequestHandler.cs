using MediaServer.Common.Mediator;
using MediaServer.Common.Threading;
using MediaServer.Core.Repositories;
using MediaServer.Models;
using NLog;
using System;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.RoomManager
{
    sealed class DeviceDisconnectionRequestHandler : IRemoteDeviceService<DeviceDisconnectionRequest>
    {
        readonly IDispatchQueue _centralDispatchQueue;
        readonly IRemoteDeviceDataRepository _remoteDeviceDataRepository;
        readonly IPeerConnectionRepository _peerConnectionRepository;
        readonly IHandler<SendStatusUpdateRequest> _statusUpdateSender;
        readonly ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        public DeviceDisconnectionRequestHandler(
            IDispatchQueue centralDispatchQueue,
            IRemoteDeviceDataRepository remoteDeviceUserProfileMappings,
            IPeerConnectionRepository peerConnectionRepository,
            IHandler<SendStatusUpdateRequest> statusUpdateSender)
        {
            _centralDispatchQueue = centralDispatchQueue
                ?? throw new ArgumentNullException(nameof(centralDispatchQueue));
            _remoteDeviceDataRepository = remoteDeviceUserProfileMappings
                ?? throw new ArgumentNullException(nameof(remoteDeviceUserProfileMappings));
            _peerConnectionRepository = peerConnectionRepository 
                ?? throw new ArgumentNullException(nameof(peerConnectionRepository));
            _statusUpdateSender = statusUpdateSender 
                ?? throw new ArgumentNullException(nameof(statusUpdateSender));
        }

        public async Task HandleAsync(IRemoteDevice remoteDevice, DeviceDisconnectionRequest request)
        {
            var deviceData = await _centralDispatchQueue.ExecuteAsync(delegate
            {
                // Remove associated PeerConnections
                foreach(var peer in _peerConnectionRepository.Find(remoteDevice))
                {
                    using(peer)
                    {
                        _peerConnectionRepository.Remove(peer);
                    }
                    _logger.Debug($"PeerConnection closed due to device disconnect, device {remoteDevice}");
                }

                // Remove device data
                var tmp = _remoteDeviceDataRepository.GetForDevice(remoteDevice);
                _remoteDeviceDataRepository.DeleteForDevice(remoteDevice);
                return tmp;
            });

            // Then jump into the room's queue and disconnectthe device with the user
            if(deviceData?.Room != null && deviceData.User != null)
            {
                await deviceData.Room.DispatchQueue.ExecuteAsync(delegate
                {
                    deviceData.User.Devices.Remove(remoteDevice);
                    _logger.Info($"Device {remoteDevice} no longer associated with user {deviceData.User}.");
                });
            }

            // Send status update so devices can update their UIs
            if(deviceData?.Room != null)
            {
                await _statusUpdateSender.HandleAsync(new SendStatusUpdateRequest
                {
                    Room = deviceData.Room
                });
            }
        }
    }
}
