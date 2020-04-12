using MediaServer.Common.Threading;
using MediaServer.Core.Common;
using MediaServer.Core.Repositories;
using MediaServer.Models;
using NLog;
using System;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.RoomManager
{
    sealed class JoinRoomRequestHandler : IRemoteDeviceRequestHandler<JoinRoomRequest, GenericResponse>
    {
        readonly IDispatchQueue _centralDispatchQueue;
        readonly IRoomRepository _roomRepository;
        readonly ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        public JoinRoomRequestHandler(IDispatchQueue centralDispatchQueue, IRoomRepository roomRepository)
        {
            _centralDispatchQueue = centralDispatchQueue 
                ?? throw new ArgumentNullException(nameof(centralDispatchQueue));
            _roomRepository = roomRepository 
                ?? throw new ArgumentNullException(nameof(roomRepository));
        }

        public async Task<GenericResponse> HandleAsync(IRemoteDevice remotDevice, JoinRoomRequest request)
        {
            if(string.IsNullOrWhiteSpace(request.Username))
                GenericResponse.ErrorResponse($"Invalid username");

            // First get the room
            var room = await _centralDispatchQueue.ExecuteAsync(delegate
            {
                return _roomRepository.GetRoomById(request.RoomId);
            });
            if(null == room)
            {
                return GenericResponse.ErrorResponse($"Room not found by id {request.RoomId}");
            }

            // Then dispatch to the room's thread and 
            // create an user profile if not exist
            return await room.DispatchQueue.ExecuteAsync(delegate
            {
                var user = room.UserProfiles.GetUserByName(request.Username);
                if(user == null)
                {
                    room.UserProfiles.AddUser(new Models.UserProfile
                    {
                        Id = Guid.NewGuid(),
                        Username = request.Username
                    });
                }
                return GenericResponse.SuccessResponse();
            });
        }
    }
}
