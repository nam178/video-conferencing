using MediaServer.Core.Common;
using MediaServer.Core.Models;

namespace MediaServer.Core.Services.ServerManager
{
    public sealed class NewRoomResponse : GenericResponse
    {
        public RoomId CreatedRoomId { get; set; }
    }
}
