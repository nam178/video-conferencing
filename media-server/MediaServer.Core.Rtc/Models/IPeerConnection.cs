using MediaServer.WebRtc.Managed;
using System;

namespace MediaServer.Rtc.Models
{
    interface IPeerConnection : IDisposable
    {
        /// <summary>
        /// Unique id of this PeerConnection, set by the client.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Updating the RTCSessionDescription
        /// </summary>
        /// <remarks>Implementation must not throw exception</remarks>
        RTCSessionDescription RemoteSessionDescription { get; set; }
    }
}
