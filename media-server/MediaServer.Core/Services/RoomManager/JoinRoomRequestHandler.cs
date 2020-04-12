using MediaServer.Common.Threading;
using MediaServer.Core.Common;
using MediaServer.Core.Errors;
using MediaServer.Core.Models;
using MediaServer.Core.Repositories;
using MediaServer.Models;
using NLog;
using System;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.RoomManager
{
    sealed class JoinRoomRequestHandler : IRemoteDeviceRequestHandler<JoinRoomRequest, GenericResponse>
    {
        readonly IDispatchQueue _centralDispatchQueue;
        readonly IRoomRepository _roomRepository;
        readonly IRemoteDeviceUserProfileMappings _remoteDeviceData;
        readonly static ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        public JoinRoomRequestHandler(
            IDispatchQueue centralDispatchQueue,
            IRoomRepository roomRepository,
            IRemoteDeviceUserProfileMappings remoteDeviceData)
        {
            _centralDispatchQueue = centralDispatchQueue
                ?? throw new ArgumentNullException(nameof(centralDispatchQueue));
            _roomRepository = roomRepository
                ?? throw new ArgumentNullException(nameof(roomRepository));
            _remoteDeviceData = remoteDeviceData
                ?? throw new ArgumentNullException(nameof(remoteDeviceData));
        }

        public async Task<GenericResponse> HandleAsync(IRemoteDevice remoteDevice, JoinRoomRequest request)
        {
            if(string.IsNullOrWhiteSpace(request.Username))
                GenericResponse.ErrorResponse($"Invalid username");

            var room = await GetAndValidateRoom(remoteDevice, request);
            if(null == room)
                return GenericResponse.ErrorResponse($"Room not found by id {request.RoomId}");

            var user = await GetOrCreateUserProfile(remoteDevice, request, room);

            // Finally associate the device with the user
            await _centralDispatchQueue.ExecuteAsync(() => _remoteDeviceData.SetMappingForDevice(remoteDevice, room, user));

            _logger.Info($"Device {remoteDevice} now associated with room {room} and user {user}");
            return GenericResponse.SuccessResponse();
        }

        async Task<Room> GetAndValidateRoom(IRemoteDevice remoteDevice, JoinRoomRequest request)
        {
            // Jump into the central dispatch queue and do some validations first
            return await _centralDispatchQueue.ExecuteAsync(delegate
            {
                // First of all, one device can only be in one room at any time:
                var data = _remoteDeviceData.GetMappingForDevice(remoteDevice);
                if(data.Room != null)
                {
                    // Impossible! Not allowed
                    throw new OperationForbiddenException(
                        $"Device {remoteDevice} already belongs to room {data.Room}; " +
                        $"Not allowed to join another room");
                }

                // Then, get the room requested and associate it with this device
                var room = _roomRepository.GetRoomById(request.RoomId);
                _remoteDeviceData.SetMappingForDevice(remoteDevice, room, data.UserProfile);
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
                    user = new Models.UserProfile
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
