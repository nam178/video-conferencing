using MediaServer.Models;
using System;
using System.Globalization;

namespace MediaServer.Core.Models
{
    public struct RoomId
    {
        readonly string _roomId;

        const int MAX_LENTH = 64;

        internal RoomId(string roomId)
        {
            _roomId = roomId 
                ?? throw new ArgumentNullException(nameof(roomId));
        }

        public static RoomId FromString(string roomId)
        {
            if(string.IsNullOrWhiteSpace(roomId))
                throw new ArgumentException("message", nameof(roomId));

            roomId = roomId.Trim();
            roomId = roomId.Replace(" ", "-").Replace("_", "-");
            roomId = roomId.ToLower(CultureInfo.InvariantCulture);
            if(roomId.Length > MAX_LENTH)
            {
                throw new ArgumentException($"{nameof(roomId)} is too long");
            }
            return new RoomId(roomId);
        }

        public override bool Equals(object obj) => (obj is RoomId) && ((RoomId)obj)._roomId == _roomId;

        public override int GetHashCode() => _roomId?.GetHashCode() ?? 0;

        public override string ToString() => _roomId ?? "<empty>";
    }
}
