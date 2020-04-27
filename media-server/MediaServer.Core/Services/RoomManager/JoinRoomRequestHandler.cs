using MediaServer.Common.Mediator;
using MediaServer.Common.Threading;
using MediaServer.Core.Common;
using MediaServer.Core.Models;
using MediaServer.Core.Repositories;
using MediaServer.Models;
using NLog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.RoomManager
{
    sealed class JoinRoomRequestHandler : IRemoteDeviceService<JoinRoomRequest, GenericResponse>
    {
        readonly IDispatchQueue _centralDispatchQueue;
        readonly IRoomRepository _roomRepository;
        readonly IHandler<SendSyncMessageRequest> _statusUpdateSender;
        readonly static ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        public JoinRoomRequestHandler(
            IDispatchQueue centralDispatchQueue,
            IRoomRepository roomRepository,
            IHandler<SendSyncMessageRequest> statusUpdateSender)
        {
            _centralDispatchQueue = centralDispatchQueue
                ?? throw new ArgumentNullException(nameof(centralDispatchQueue));
            _roomRepository = roomRepository
                ?? throw new ArgumentNullException(nameof(roomRepository));
            _statusUpdateSender = statusUpdateSender 
                ?? throw new ArgumentNullException(nameof(statusUpdateSender));
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
            deviceData.User = user;
            remoteDevice.SetCustomData(deviceData);
            _logger.Info($"Device {remoteDevice} now associated with room {room} and user {user}");

            // Broadcast the status update
            await _statusUpdateSender.HandleAsync(new SendSyncMessageRequest
            {
                Room = room
            });
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
