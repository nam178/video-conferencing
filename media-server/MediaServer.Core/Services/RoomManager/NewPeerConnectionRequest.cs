using MediaServer.Core.Models;

namespace MediaServer.Core.Services.RoomManager
{
    public sealed class NewPeerConnectionRequest
    {
        /// <summary>
        /// The user i which this peer connection is for
        /// </summary>
        public User User { get; }
    }
}