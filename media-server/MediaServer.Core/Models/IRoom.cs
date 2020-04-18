using MediaServer.Common.Threading;
using MediaServer.Core.Models;
using MediaServer.Core.Repositories;

namespace MediaServer.Models
{
    /// <summary>
    /// The room model.
    /// </summary>
    /// <remarks>Not thread safe. Make sure modifications to the model are made using the dispatch queue associated with this room</remarks>
    public interface IRoom
    {
        /// <summary>
        /// Each room has its own dispatch queue, used to update models, handling signals, etc..
        /// to avoid race conditions.
        /// </summary>
        IDispatchQueue DispatchQueue { get; }

        /// <summary>
        /// The room id
        /// </summary>
        RoomId Id { get; set; }

        /// <summary>
        /// The factory to create PeerConnections for users in this room.
        /// </summary>
        IPeerConnectionFactory PeerConnectionFactory { get; }

        /// <summary>
        /// The users that belongs to this room
        /// </summary>
        IUserProfileCollection UserProfiles { get; }
    }
}