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
        readonly IRemoteDeviceUserProfileMappings _remoteDeviceUserProfileMappings;
        readonly IHandler<SendStatusUpdateRequest> _statusUpdateSender;
        readonly ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        public DeviceDisconnectionRequestHandler(
            IDispatchQueue centralDispatchQueue,
            IRemoteDeviceUserProfileMappings remoteDeviceUserProfileMappings,
            IHandler<SendStatusUpdateRequest> statusUpdateSender)
        {
            _centralDispatchQueue = centralDispatchQueue
                ?? throw new ArgumentNullException(nameof(centralDispatchQueue));
            _remoteDeviceUserProfileMappings = remoteDeviceUserProfileMappings
                ?? throw new ArgumentNullException(nameof(remoteDeviceUserProfileMappings));
            _statusUpdateSender = statusUpdateSender 
                ?? throw new ArgumentNullException(nameof(statusUpdateSender));
        }

        public async Task HandleAsync(IRemoteDevice remoteDevice, DeviceDisconnectionRequest request)
        {
            // First, jump into the central queue and disconnect the device with the room
            var previousMappings = await _centralDispatchQueue.ExecuteAsync(delegate
            {
                var previousMappings = _remoteDeviceUserProfileMappings.GetMappingForDevice(remoteDevice);
                _remoteDeviceUserProfileMappings.DeleteMappingForDevice(remoteDevice);
                return previousMappings;
            });

            // Then jump into the room's queue and disconnectthe device with the user
            if(previousMappings.Room != null && previousMappings.UserProfile != null)
            {
                await previousMappings.Room.DispatchQueue.ExecuteAsync(delegate
                {
                    previousMappings.UserProfile.Devices.Remove(remoteDevice);
                    _logger.Info($"Device {remoteDevice} no longer associated with user {previousMappings.UserProfile}.");
                });
            }

            // Send status update so devices can update their uis
            if(previousMappings.Room != null)
            {
                await _statusUpdateSender.HandleAsync(new SendStatusUpdateRequest
                {
                    Room = previousMappings.Room
                });
            }
        }
    }
}
