using MediaServer.Common.Utils;
using System;
using System.Collections.Generic;

namespace MediaServer.Models
{
    sealed class Peer
    {
        RtcSessionDescription _remoteSessionDescription;
        IReadOnlyList<IceCandidate> _remoteIceCandidates = new List<IceCandidate>();
        readonly object _syncRoot = new object();

        public event EventHandler<EventArgs<IceCandidate>> RemoteIceCandidateReceived;
        public event EventHandler<EventArgs<RtcSessionDescription>> RemoteRtcSessionDescriptionUpdated;

        public IReadOnlyList<IceCandidate> RemoteIceCandidates => _remoteIceCandidates;

        public RtcSessionDescription RemoteRtcSessionDescription
        {
            get => _remoteSessionDescription;
            set
            {
                _remoteSessionDescription = value;
                RemoteRtcSessionDescriptionUpdated?.Invoke(this, new EventArgs<RtcSessionDescription>(value));
            }
        }

        public ISignaller Signaller { get; }

        public string Name { get; }

        public Peer(string name, ISignaller peerMessenger)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Signaller = peerMessenger ?? throw new ArgumentNullException(nameof(peerMessenger));
        }

        public void ReceiveRemoteCandidate(IceCandidate iceCandidate)
        {
            CopyOnWrite.Add(ref _remoteIceCandidates, iceCandidate, _syncRoot);
            RemoteIceCandidateReceived?.Invoke(this, new EventArgs<IceCandidate>(iceCandidate));
        }

        public override string ToString() => $"[Peer {Name}]";
    }
}
