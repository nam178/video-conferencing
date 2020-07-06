using MediaServer.Common.Patterns;
using MediaServer.WebRtc.Common;
using MediaServer.WebRtc.Managed;
using System;

namespace MediaServer.Core.Models
{
    /// <remarks>
    /// Properties are thread safe, but methods must be called from signalling thread.
    /// </remarks>
    public interface IPeerConnection : IDisposable
    {
        IRoom Room { get; }

        IRemoteDevice Device { get; }

        Guid Id { get; }

        Guid LastOfferId { get; }

        internal PeerConnection Native { get; }

        void CreateAnswer(Callback<RTCSessionDescription> callback);

        void CreateOffer(Callback<RTCSessionDescription> callback);

        void SetRemoteSessionDescription(RTCSessionDescription description, Callback callback);

        void SetLocalSessionDescription(RTCSessionDescription description, Callback callback);

        IPeerConnection ObserveIceCandidate(Action<IPeerConnection, RTCIceCandidate> observer);

        IPeerConnection ObserveRenegotiationNeeded(Action<IPeerConnection> observer);

        void AddIceCandidate(RTCIceCandidate iceCandidate);

        void Close();
    }
}
