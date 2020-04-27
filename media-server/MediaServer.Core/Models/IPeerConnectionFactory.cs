using MediaServer.Common.Threading;
using MediaServer.Models;

namespace MediaServer.Core.Models
{
    public interface IPeerConnectionFactory
    {
        /// <summary>
        /// Get the signalling thread
        /// </summary>
        /// <remarks>This is the same signalling thread with the room</remarks>
        /// <exception cref="System.InvalidOperationException">When this PeerConnectionFactory has not been initialised</exception>
        IDispatchQueue SignallingThread { get; }

        void Initialize();

        IPeerConnection Create(IRemoteDevice remoteDevice, IRoom room);
    }
}
