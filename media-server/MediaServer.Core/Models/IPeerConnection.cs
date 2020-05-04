using MediaServer.Models;
using MediaServer.WebRtc.Managed;
using System;
using System.Threading.Tasks;

namespace MediaServer.Core.Models
{
    /// <remarks>
    /// This PeerConnection is thread safe. Members can be called/accessed from any thread.
    /// The underlying native WebRTC implementation says it will proxy to the correct thread.
    /// </remarks>
    public interface IPeerConnection : IDisposable
    {
        public IRoom Room { get; }

        public IRemoteDevice Device { get; }

        public Guid Id { get; }

        Task StartMediaRoutingAsync();

        Task<RTCSessionDescription> CreateAnswerAsync();

        Task<RTCSessionDescription> CreateOfferAsync();

        Task SetRemoteSessionDescriptionAsync(RTCSessionDescription description);

        Task SetLocalSessionDescriptionAsync(RTCSessionDescription description);

        IPeerConnection ObserveIceCandidate(Action<IPeerConnection, RTCIceCandidate> observer);

        IPeerConnection ObserveRenegotiationNeeded(Action<IPeerConnection> observer);

        void AddIceCandidate(RTCIceCandidate iceCandidate);

        Task CloseAsync();
    }
}
