using MediaServer.Common.Utils;
using MediaServer.WebRtc.Managed;
using System;
using System.Collections.Generic;

namespace MediaServer.Models
{
    sealed class Peer
    {
        RTCSessionDescription _remoteSessionDescription;
        IReadOnlyList<RTCIceCandidate> _remoteIceCandidates = new List<RTCIceCandidate>();
        readonly object _syncRoot = new object();

        public event EventHandler<EventArgs<RTCIceCandidate>> RemoteIceCandidateReceived;
        public event EventHandler<EventArgs<RTCSessionDescription>> RemoteRtcSessionDescriptionUpdated;

        public IReadOnlyList<RTCIceCandidate> RemoteIceCandidates => _remoteIceCandidates;

        public RTCSessionDescription RemoteRtcSessionDescription
        {
            get => _remoteSessionDescription;
            set
            {
                _remoteSessionDescription = value;
                RemoteRtcSessionDescriptionUpdated?.Invoke(this, new EventArgs<RTCSessionDescription>(value));
            }
        }

        public ISignaller Signaller { get; }

        public string Name { get; }

        public Peer(string name, ISignaller peerMessenger)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Signaller = peerMessenger ?? throw new ArgumentNullException(nameof(peerMessenger));
        }

        public void ReceiveRemoteCandidate(RTCIceCandidate iceCandidate)
        {
            CopyOnWrite.Add(ref _remoteIceCandidates, iceCandidate, _syncRoot);
            RemoteIceCandidateReceived?.Invoke(this, new EventArgs<RTCIceCandidate>(iceCandidate));
        }

        public override string ToString() => $"[Peer {Name}]";
    }
}
