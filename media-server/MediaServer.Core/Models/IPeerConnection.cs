using MediaServer.WebRtc.Managed;
using System;

namespace MediaServer.Core.Models
{
    /// <summary>
    /// Implementation will proxy this to the desired tech, for instance it's WebRTC.
    /// </summary>
    /// <remarks>
    /// This PeerConnection is thread safe. Members can be called/accessed from any thread.
    /// The underlying native WebRTC implementation says it will proxy to the correct thread.
    /// </remarks>
    public interface IPeerConnection : IDisposable
    {
        public RTCSessionDescription RemoteSessionDescription { get; set; }
    }
}
