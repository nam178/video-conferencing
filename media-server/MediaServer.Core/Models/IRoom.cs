using MediaServer.Common.Threading;
using MediaServer.Core.Models;
using MediaServer.Core.Repositories;
using MediaServer.WebRtc.MediaRouting;
using System.Threading.Tasks;

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
        /// <exception cref="System.InvalidOperationException">When the room is not initialised</exception>
        IDispatchQueue SignallingThread { get; }

        IDispatchQueue RenegotiationQueue { get; }

        /// <summary>   
        /// The room id
        /// </summary>
        RoomId Id { get; }

        /// <summary>
        /// Use this to check whenever the room is initialised and ready for using
        /// </summary>
        RoomState State { get; }

        /// <summary>
        /// The users that belongs to this room
        /// </summary>
        /// <exception cref="System.InvalidOperationException">When the room is not initialised</exception>
        IUserProfileCollection UserProfiles { get; }

        /// <summary>
        /// Video router that routes video for this room
        /// </summary>
        IVideoRouter VideoRouter { get; }

        /// <summary>
        /// Initialise the room
        /// </summary>
        void Initialize();

        /// <summary>
        /// Create a PeerConnection for specified remote device
        /// </summary>
        /// <param name="remoteDevice"></param>
        /// <returns></returns>
        Task<IPeerConnection> CreatePeerConnectionAsync(IRemoteDevice remoteDevice);
    }
}