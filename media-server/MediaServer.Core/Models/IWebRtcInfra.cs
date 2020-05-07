using MediaServer.Common.Threading;
using MediaServer.Models;
using MediaServer.WebRtc.MediaRouting;
using System.Threading.Tasks;

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

        void Initialize();

        IPeerConnection CreatePeerConnection(IRemoteDevice remoteDevice, IRoom room);
    }
}
