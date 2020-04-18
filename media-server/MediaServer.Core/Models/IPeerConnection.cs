using MediaServer.WebRtc.Managed;
using System;
using System.Threading.Tasks;

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
        public Task SetRemoteSessionDescriptionAsync(RTCSessionDescription description);
    }
}
