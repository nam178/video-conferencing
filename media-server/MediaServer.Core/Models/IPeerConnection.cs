using MediaServer.Common.Patterns;
using MediaServer.Models;
using MediaServer.WebRtc.Managed;
using System;

namespace MediaServer.Core.Models
{
    /// <remarks>
    /// Properties are thread safe, but methods must be called from signalling thread.
    /// </remarks>
    public interface IPeerConnection : IDisposable
    {
        public IRoom Room { get; }

        public IRemoteDevice Device { get; }

        public Guid Id { get; }

        void CreateAnswer(Observer<RTCSessionDescription> observer);

        void CreateOffer(Observer<RTCSessionDescription> observer);

        void SetRemoteSessionDescription(RTCSessionDescription description, Observer observer);

        void SetLocalSessionDescription(RTCSessionDescription description, Observer observer);

        IPeerConnection ObserveIceCandidate(Action<IPeerConnection, RTCIceCandidate> observer);

        IPeerConnection ObserveRenegotiationNeeded(Action<IPeerConnection> observer);

        void AddIceCandidate(RTCIceCandidate iceCandidate);

        void Close();
    }
}
