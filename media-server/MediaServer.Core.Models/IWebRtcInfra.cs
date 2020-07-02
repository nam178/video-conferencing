using MediaServer.Common.Threading;
using MediaServer.Models;

namespace MediaServer.Core.Models
{
    /// <summary>
    /// Represents the WebRTC library
    /// </summary>
    interface IWebRtcInfra
    {
        /// <summary>
        /// Get the signalling thread
        /// </summary>
        /// <remarks>This is the same signalling thread with the room</remarks>
        /// <exception cref="System.InvalidOperationException">When this PeerConnectionFactory has not been initialised</exception>
        IThread SignallingThread { get; }

        /// <summary>
        /// The VideoRouter, use this to route video between video clients.
        /// </summary>
        IVideoRouter VideoRouter { get; }

        /// <summary>
        /// Initialise the library
        /// </summary>
        /// <exception cref="System.InvalidOperationException">When the library already initialised</exception>
        void Initialize();

        /// <summary>
        /// Create a new PeerConnection with the specified device
        /// </summary>
        /// <param name="remoteDevice"></param>
        /// <param name="room"></param>
        /// <returns></returns>
        IPeerConnection CreatePeerConnection(IRemoteDevice remoteDevice, IRoom room);
    }
}
