using MediaServer.Common.Threading;
using MediaServer.Core.Services.MediaRouting;
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
        IDispatchQueue SignallingThread { get; }

        void Initialize();

        IPeerConnection CreatePeerConnection(IRemoteDevice remoteDevice, IRoom room);

        IVideoRouter CreateVideoRouter();
    }
}
