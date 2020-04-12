using MediaServer.Core.Models;
using MediaServer.Models;
using System;
using System.Collections.Generic;

namespace MediaServer.Core.Repositories
{
    sealed class RoomRepository : IRoomRepository
    {
        /// <summary>
        /// Active rooms that being managed by this servie
        /// </summary>
        readonly List<Room> _rooms = new List<Room>();
        readonly Dictionary<RoomId, Room> _roomIndexById = new Dictionary<RoomId, Room>();

        /// <summary>
        /// Create new room
        /// </summary>
        /// <param name="id"></param>
        /// <exception cref="InvalidOperationException">Room exists</exception>"
        /// <returns></returns>
        public Room CreateRoom(RoomId id)
        {
            if(_roomIndexById.ContainsKey(id))
            {
                throw new InvalidOperationException($"Room by id {id} already exist");
            }
            _roomIndexById[id] = new Room();
            return _roomIndexById[id];
        }
    }
}
