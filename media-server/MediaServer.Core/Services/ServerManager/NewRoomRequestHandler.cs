using MediaServer.Common.Threading;
using MediaServer.Core.Models;
using MediaServer.Core.Repositories;
using MediaServer.Models;
using NLog;
using System;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.ServerManager
{
    sealed class NewRoomRequestHandler : ICoreService<NewRoomRequest, RoomId>
    {
        readonly IDispatchQueue _centralDispatchQueue;
        readonly IRoomRepository _roomRepository;
        readonly IRoomFactory _roomFactory;
        readonly ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        public NewRoomRequestHandler(
            IDispatchQueue centralDispatchQueue,
            IRoomRepository roomRepository,
            IRoomFactory roomFactory)
        {
            _centralDispatchQueue = centralDispatchQueue
                ?? throw new ArgumentNullException(nameof(centralDispatchQueue));
            _roomRepository = roomRepository
                ?? throw new ArgumentNullException(nameof(roomRepository));
            _roomFactory = roomFactory
                ?? throw new ArgumentNullException(nameof(roomFactory));
        }

        public async Task<RoomId> HandleAsync(IRemoteDevice remoteDevice, NewRoomRequest request)
        {
            // Currently anyone can create rooms,
            // TODO: add some security here
            var roomId = RoomId.FromString(request.NewRoomName);
            var room = await _centralDispatchQueue.ExecuteAsync(() =>
                {
                    var existingRoom = _roomRepository.GetRoomById(roomId);
                    if(existingRoom != null)
                    {
                        return existingRoom;
                    }
                    var newRoom = _roomFactory.Create(roomId);
                    _roomRepository.AddRoom(newRoom);
                    _logger.Info($"New room created: {newRoom}");
                    return newRoom;
                });

            // Initialise room, 
            // done in the room's queue to avoid blocking the main queue.
            await room.DispatchQueue.ExecuteAsync(delegate
            {
                room.PeerConnectionFactory.EnsureInitialised();
            });

            return roomId;
        }
    }
}
