using MediaServer.Core.Models;

namespace MediaServer.Core.Services.RoomManagement
{
    public sealed class JoinRoomRequest
    {
        /// <summary>
        /// The room in which the client wishes to join
        /// </summary>
        public RoomId RoomId { get; set; }

        /// <summary>
        /// The username in which the client is identifing themshelves as
        /// </summary>
        public string Username { get; set; }
    }
}
