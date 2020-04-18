using MediaServer.Models;

namespace MediaServer.Core.Services.RoomManager
{
    sealed class SendStatusUpdateRequest
    {
        /// <summary>
        /// The room in which the status update request will be sent to
        /// </summary>
        public IRoom Room { get; set; }
    }
}