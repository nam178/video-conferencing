using MediaServer.Core.Models;

namespace MediaServer.Core.Services.RoomManagement
{
    public sealed class NewRoomResponse : GenericResponse
    {
        public RoomId CreatedRoomId { get; set; }
    }
}
