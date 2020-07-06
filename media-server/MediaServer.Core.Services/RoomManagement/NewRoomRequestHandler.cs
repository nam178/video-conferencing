using MediaServer.Common.Patterns;
using MediaServer.Common.Threading;
using MediaServer.Core.Models;
using MediaServer.Core.Models.Repositories;
using NLog;
using System;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.RoomManagement
{
    sealed class NewRoomRequestHandler : IMapper<IRemoteDevice, NewRoomRequest, RoomId>
    {
        readonly IDispatchQueue _centralDispatchQueue;
        readonly IRoomRepository _roomRepository;
        readonly IRoomFactory _roomFactory;
        readonly IObserver<TransceiverMetadataUpdatedEvent> _transceiverMetadataUpdatedObserver;
        readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public NewRoomRequestHandler(
            IDispatchQueue centralDispatchQueue,
            IRoomRepository roomRepository,
            IRoomFactory roomFactory,
            IObserver<TransceiverMetadataUpdatedEvent> transceiverMetadataUpdatedObserver)
        {
            _centralDispatchQueue = centralDispatchQueue
                ?? throw new ArgumentNullException(nameof(centralDispatchQueue));
            _roomRepository = roomRepository
                ?? throw new ArgumentNullException(nameof(roomRepository));
            _roomFactory = roomFactory
                ?? throw new ArgumentNullException(nameof(roomFactory));
            _transceiverMetadataUpdatedObserver = transceiverMetadataUpdatedObserver
                ?? throw new ArgumentNullException(nameof(transceiverMetadataUpdatedObserver));
        }

        public async Task<RoomId> HandleAsync(IRemoteDevice remoteDevice, NewRoomRequest request)
        {
            // Currently anyone can create rooms,
            // TODO: add some security here
            var isNewRoomCreated = false;
            var roomId = RoomId.FromString(request.NewRoomName);
            var room = await _centralDispatchQueue.ExecuteAsync(() =>
                {
                    var existingRoom = _roomRepository.GetRoomById(roomId);
                    if(existingRoom != null)
                    {
                        if(existingRoom.State != RoomState.Ok)
                            throw new InvalidProgramException(
                                "Clients should not be able to join rooms while they are being initialised"
                                );
                        return existingRoom;
                    }

                    var newRoom = _roomFactory.Create(roomId);
                    _roomRepository.AddRoom(newRoom);
                    _logger.Info($"New room created: {newRoom}");
                    isNewRoomCreated = true;
                    return newRoom;
                });


            // The device that creates the room is responsible for initializing the room.
            if(isNewRoomCreated)
            {
                room.Initialize();
                room.VideoRouter.Subscribe(_transceiverMetadataUpdatedObserver);
            }

            return roomId;
        }
    }
}