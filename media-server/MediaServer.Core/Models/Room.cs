using MediaServer.Common.Threading;
using MediaServer.Core.Models;
using MediaServer.Core.Repositories;

namespace MediaServer.Models
{
    /// <summary>
    /// The room model.
    /// </summary>
    /// <remarks>Not thread safe. Make sure modifications to the model are made using the dispatch queue associated with this room</remarks>
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

        /// <summary>
        /// The users that belongs to this room
        /// </summary>
        public IUserProfileCollection UserProfiles { get; } = new UserProfileCollection();

        public Room()
        {
            // Create and start an exclusive dispatch queue for this room
            DispatchQueue = new ThreadPoolDispatchQueue();
            ((ThreadPoolDispatchQueue)DispatchQueue).Start();
        }

        public override string ToString() => $"[Room Id={Id}]";
    }
}
