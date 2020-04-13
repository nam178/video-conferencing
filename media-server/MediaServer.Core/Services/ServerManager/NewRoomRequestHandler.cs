using MediaServer.Common.Threading;
using MediaServer.Core.Models;
using MediaServer.Core.Repositories;
using MediaServer.Models;
using System;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.ServerManager
{
    sealed class NewRoomRequestHandler : IRemoteDeviceRequestHandler<NewRoomRequest, NewRoomResponse>
    {
        readonly IDispatchQueue _centralDispatchQueue;
        readonly IRoomRepository _roomRepository;

        public NewRoomRequestHandler(IDispatchQueue centralDispatchQueue, IRoomRepository roomRepository)
        {
            _centralDispatchQueue = centralDispatchQueue
                ?? throw new ArgumentNullException(nameof(centralDispatchQueue));
            _roomRepository = roomRepository
                ?? throw new ArgumentNullException(nameof(roomRepository));
        }

        public Task<NewRoomResponse> HandleAsync(IRemoteDevice remoteDevice, NewRoomRequest request)
        {
            // Currently anyone can create rooms,
            // TODO: add some security here
            var roomId = RoomId.FromString(request.NewRoomName);
            return _centralDispatchQueue.ExecuteAsync(delegate
            {
                try
                {
                    var room = _roomRepository.CreateRoom(roomId);
                    return new NewRoomResponse // room created the first time
                    {
                        Success = true,
                        CreatedRoomId = room.Id
                    };
                }
                catch(InvalidOperationException) // room already created
                {
                    return new NewRoomResponse { Success = true, CreatedRoomId = roomId };
                }
                catch(Exception ex) // unexpected failure
                {
                    return new NewRoomResponse
                    {
                        Success = false,
                        ErrorMessage = ex.Message
                    };
                }
            });
        }
    }
}
