using MediaServer.Core.Models;
using MediaServer.Models;
using MediaServer.Rtc.Models;
using System.Collections.Generic;

namespace MediaServer.Rtc.Repositories
{
    /// <summary>
    /// Keeping record of created peer connections
    /// </summary>
    /// <remarks>
    /// Not thread safe, designed to be a singleton. Access from central dispatch quue.
    /// </remarks>
    interface IPeerConnectionRepository
    {
        /// <summary>
        /// Register a PeerConnection that associate with the specified user and remote device
        /// </summary>
        /// <param name="user"></param>
        /// <param name="remoteDevice"></param>
        /// <param name="peerConnection"></param>
        void Add(UserProfile user, IRemoteDevice remoteDevice, IPeerConnection peerConnection);

        /// <summary>
        /// Find the registered PeerConnection that associated with remote device
        /// </summary>
        /// <param name="user"></param>
        /// <param name="remoteDevice"></param>
        /// <returns></returns>
        IReadOnlyList<IPeerConnection> Find(IRemoteDevice remoteDevice);

        /// <summary>
        /// Remove the specified peer connection
        /// </summary>
        /// <param name="peerConnection"></param>
        void Remove(IPeerConnection peerConnection);
    }
}
