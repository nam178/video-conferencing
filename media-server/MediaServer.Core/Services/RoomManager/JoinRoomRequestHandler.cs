using MediaServer.Common.Mediator;
using MediaServer.Common.Threading;
using MediaServer.Core.Common;
using MediaServer.Core.Errors;
using MediaServer.Core.Models;
using MediaServer.Core.Repositories;
using MediaServer.Models;
using NLog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.RoomManager
{
    sealed class JoinRoomRequestHandler : ICoreService<JoinRoomRequest, GenericResponse>
    {
        readonly IDispatchQueue _centralDispatchQueue;
        readonly IRoomRepository _roomRepository;
        readonly IRemoteDeviceDataRepository _remoteDeviceDataRepository;
        readonly IHandler<SendStatusUpdateRequest> _statusUpdateSender;
        readonly static ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        public JoinRoomRequestHandler(
            IDispatchQueue centralDispatchQueue,
            IRoomRepository roomRepository,
            IRemoteDeviceDataRepository remoteDeviceDataRepository,
            IHandler<SendStatusUpdateRequest> statusUpdateSender)
        {
            _centralDispatchQueue = centralDispatchQueue
                ?? throw new ArgumentNullException(nameof(centralDispatchQueue));
            _roomRepository = roomRepository
                ?? throw new ArgumentNullException(nameof(roomRepository));
            _remoteDeviceDataRepository = remoteDeviceDataRepository
                ?? throw new ArgumentNullException(nameof(remoteDeviceDataRepository));
            _statusUpdateSender = statusUpdateSender 
                ?? throw new ArgumentNullException(nameof(statusUpdateSender));
        }

        public async Task<GenericResponse> HandleAsync(IRemoteDevice remoteDevice, JoinRoomRequest request)
        {
            if(string.IsNullOrWhiteSpace(request.Username))
                GenericResponse.ErrorResponse($"Invalid username");

            // Get the room and user
            var room = await GetAndValidateRoom(remoteDevice, request);
            if(null == room)
                return GenericResponse.ErrorResponse($"Room not found by id {request.RoomId}");
            var user = await GetOrCreateUserProfile(remoteDevice, request, room);

            // Associate the device with room/user
            await _centralDispatchQueue.ExecuteAsync(() 
                => _remoteDeviceDataRepository.SetForDevice(
                    remoteDevice, 
                    g => g.User = user));
            _logger.Info($"Device {remoteDevice} now associated with room {room} and user {user}");

            // Broadcast the status update
            await _statusUpdateSender.HandleAsync(new SendStatusUpdateRequest
            {
                Room = room
            });
            return GenericResponse.SuccessResponse();
        }

        async Task<Room> GetAndValidateRoom(IRemoteDevice remoteDevice, JoinRoomRequest request)
        {
            // Jump into the central dispatch queue and do some validations first
            return await _centralDispatchQueue.ExecuteAsync(delegate
            {
                // First of all, one device can only be in one room at any time:
                var data = _remoteDeviceDataRepository.GetForDevice(remoteDevice);
                if(data?.Room != null)
                {
                    // Impossible! Not allowed
                    throw new OperationForbiddenException(
                        $"Device {remoteDevice} already belongs to room {data.Room}; " +
                        $"Not allowed to join another room");
                }

                // Then, get the room requested and associate it with this device
                var room = _roomRepository.GetRoomById(request.RoomId);
                _remoteDeviceDataRepository.SetForDevice(remoteDevice, t => t.Room = room);
                return room;
            });
        }

        static async Task<UserProfile> GetOrCreateUserProfile(IRemoteDevice remoteDevice, JoinRoomRequest request, Room room)
        {
            // Then dispatch to the room's thread and 
            // create an user profile if not exist
            return await room.DispatchQueue.ExecuteAsync(delegate
            {
                var user = room.UserProfiles.GetUserByName(request.Username);
                if(user == null)
                {
                    user = new UserProfile(room)
                    {
                        Id = Guid.NewGuid(),
                        Username = request.Username,
                        Devices = { remoteDevice }
                    };
                    room.UserProfiles.AddUser(user);
                    _logger.Info($"New user profile created: {user}");
                }
                else
                {
                    // User already exists,
                    // add the device if it's not added already
                    if(!user.Devices.Contains(remoteDevice))
                    {
                        user.Devices.Add(remoteDevice);
                    }
                }
                return user;
            });
        }
    }
}
