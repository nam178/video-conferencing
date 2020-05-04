using MediaServer.Common.Mediator;
using MediaServer.Core.Common;
using MediaServer.Core.Models;
using MediaServer.Core.Repositories;
using MediaServer.Models;
using NLog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.RoomManagement
{
    sealed class JoinRoomRequestHandler : IMapper<IRemoteDevice, JoinRoomRequest, GenericResponse>
    {
        readonly IRoomRepository _roomRepository;
        readonly ISyncMessenger _syncMessenger;
        readonly static ILogger _logger = LogManager.GetCurrentClassLogger();

        public JoinRoomRequestHandler(
            IRoomRepository roomRepository,
            ISyncMessenger syncMessenger)
        {
            _roomRepository = roomRepository
                ?? throw new ArgumentNullException(nameof(roomRepository));
            _syncMessenger = syncMessenger
                ?? throw new ArgumentNullException(nameof(syncMessenger));
        }

        public async Task<GenericResponse> HandleAsync(IRemoteDevice remoteDevice, JoinRoomRequest request)
        {
            if(string.IsNullOrWhiteSpace(request.Username))
                GenericResponse.ErrorResponse($"Invalid username");

            // Get the room and user
            var deviceData = remoteDevice.GetCustomData();
            if(deviceData.Room != null)
            {
                throw new InvalidOperationException(
                    $"Device {remoteDevice} already belongs to room {deviceData.Room}; " +
                    $"Not allowed to join another room");
            }

            var room = _roomRepository.GetRoomById(request.RoomId);
            if(null == room)
                return GenericResponse.ErrorResponse($"Room not found by id {request.RoomId}");
            var user = await GetOrCreateUserProfile(remoteDevice, request, room);

            // Associate the device with room/user
            deviceData.Room = room;
            deviceData.User = user;
            remoteDevice.SetCustomData(deviceData);
            _logger.Info($"Device {remoteDevice} now associated with room {room} and user {user}");

            // Add this device to the router, 
            // so the router knows about it and will route media to it.
            await deviceData.Room.VideoRouter.AddVideoClientAsync(remoteDevice.Id);

            // Broadcast the status update
            await _syncMessenger.SendAsync(room);
            return GenericResponse.SuccessResponse();
        }

        static async Task<User> GetOrCreateUserProfile(IRemoteDevice remoteDevice, JoinRoomRequest request, IRoom room)
        {
            // Then dispatch to the room's thread and 
            // create an user profile if not exist
            return await room.SignallingThread.ExecuteAsync(delegate
            {
                var user = room.UserProfiles.GetUserByName(request.Username);
                if(user == null)
                {
                    user = new User(room)
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
