using MediaServer.Common.Threading;
using MediaServer.Core.Models;

namespace MediaServer.Models
{
    public sealed class Room
    {
        /// <summary>
        /// Each room has its own dispatch queue, used to update models, handling signals, etc..
        /// to avoid race conditions.
        /// </summary>
        public IDispatchQueue DispatchQueue { get; }

        /// <summary>
        /// The room id
        /// </summary>
        public RoomId Id { get; set; }

        public Room()
        {
            // Create and start an exclusive dispatch queue for this room
            DispatchQueue = new ThreadPoolDispatchQueue();
            ((ThreadPoolDispatchQueue)DispatchQueue).Start();
        }
    }
}
