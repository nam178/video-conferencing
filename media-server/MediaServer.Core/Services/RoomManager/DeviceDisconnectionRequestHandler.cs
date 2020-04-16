using MediaServer.Common.Mediator;
using MediaServer.Common.Threading;
using MediaServer.Core.Repositories;
using MediaServer.Models;
using NLog;
using System;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.RoomManager
{
    sealed class DeviceDisconnectionRequestHandler : IRemoteDeviceRequestHandler<DeviceDisconnectionRequest>
    {
        readonly IDispatchQueue _centralDispatchQueue;
        readonly IRemoteDeviceDataRepository _remoteDeviceDataRepository;
        readonly IHandler<SendStatusUpdateRequest> _statusUpdateSender;
        readonly ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        public DeviceDisconnectionRequestHandler(
            IDispatchQueue centralDispatchQueue,
            IRemoteDeviceDataRepository remoteDeviceUserProfileMappings,
            IHandler<SendStatusUpdateRequest> statusUpdateSender)
        {
            _centralDispatchQueue = centralDispatchQueue
                ?? throw new ArgumentNullException(nameof(centralDispatchQueue));
            _remoteDeviceDataRepository = remoteDeviceUserProfileMappings
                ?? throw new ArgumentNullException(nameof(remoteDeviceUserProfileMappings));
            _statusUpdateSender = statusUpdateSender 
                ?? throw new ArgumentNullException(nameof(statusUpdateSender));
        }

        public async Task HandleAsync(IRemoteDevice remoteDevice, DeviceDisconnectionRequest request)
        {
            // First, jump into the central queue and disconnect the device with the room
            var deviceData = await _centralDispatchQueue.ExecuteAsync(delegate
            {
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

            // Send status update so devices can update their uis
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
