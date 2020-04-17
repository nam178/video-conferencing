using MediaServer.Core.Models;
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
        /// <param name="peerConnection"></param>
        void Add(UserProfile user, IPeerConnection peerConnection);

        /// <summary>
        /// Find the registered PeerConnection that associated with the user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        IReadOnlyList<IPeerConnection> Find(UserProfile user);
    }
}
