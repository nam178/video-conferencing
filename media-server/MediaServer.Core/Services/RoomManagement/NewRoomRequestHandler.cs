using MediaServer.Common.Threading;
using MediaServer.Core.Common;
using MediaServer.Core.Models;
using MediaServer.Core.Repositories;
using MediaServer.Models;
using System;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.RoomManagement
{
    sealed class NewRoomRequestHandler : IRemoteDeviceRequestHandler<NewRoomRequest, NewRoomResponse>
    {
        readonly IDispatchQueue _globalDispatchQueue;
        readonly IRoomRepository _roomRepository;

        public NewRoomRequestHandler(IDispatchQueue globalDispatchQueue, IRoomRepository roomRepository)
        {
            _globalDispatchQueue = globalDispatchQueue
                ?? throw new ArgumentNullException(nameof(globalDispatchQueue));
            _roomRepository = roomRepository
                ?? throw new ArgumentNullException(nameof(roomRepository));
        }

        public Task<NewRoomResponse> HandleAsync(IRemoteDevice remoteDevice, NewRoomRequest request)
        {
            // Currently anyone can create rooms,
            // TODO: add some security here

            return _globalDispatchQueue.ExecuteAsync(delegate
            {
                try
                {
                    var room = _roomRepository.CreateRoom(RoomId.FromString(request.NewRoomName));
                    return new NewRoomResponse
                    {
                        Success = true,
                        CreatedRoomId = room.Id
                    };
                }
                catch(Exception ex)
                {
                    return new NewRoomResponse
                    {
                        Success = false,
                        ErrorMessage = ex.ToString()
                    };
                }
            });
        }
    }
}
