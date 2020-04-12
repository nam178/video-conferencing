using MediaServer.Core.Models;
using MediaServer.Models;
using System;
using System.Collections.Generic;

namespace MediaServer.Core.Repositories
{
    sealed class RoomRepository : IRoomRepository
    {
        readonly List<Room> _rooms = new List<Room>();
        readonly Dictionary<RoomId, Room> _roomIndexById = new Dictionary<RoomId, Room>();

        public Room CreateRoom(RoomId id)
        {
            if(_roomIndexById.ContainsKey(id))
            {
                throw new InvalidOperationException($"Room by id {id} already exist");
            }
            var room = new Room();
            _roomIndexById[id] = room;
            _rooms.Add(room);
            return _roomIndexById[id];
        }

        public Room GetRoomById(RoomId id)
        {
            if(_roomIndexById.ContainsKey(id))
            {
                return _roomIndexById[id];
            }
            return default;
        }
    }
}
