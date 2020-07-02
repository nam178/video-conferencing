using MediaServer.Common.Utils;
using System;
using System.Collections.Generic;

namespace MediaServer.Core.Models.Repositories
{
    sealed class RoomRepository : IRoomRepository
    {
        readonly List<IRoom> _rooms = new List<IRoom>();
        readonly Dictionary<RoomId, IRoom> _roomIndexById = new Dictionary<RoomId, IRoom>();

        public void AddRoom(IRoom room)
        {
            Require.NotNull(room);
            if(_roomIndexById.ContainsKey(room.Id))
            {
                throw new InvalidOperationException($"Room by id {room.Id} already exist");
            }
            _roomIndexById[room.Id] = room;
            _rooms.Add(room);
        }

        public IRoom GetRoomById(RoomId id)
        {
            if(_roomIndexById.ContainsKey(id))
            {
                return _roomIndexById[id];
            }
            return default;
        }
    }
}
