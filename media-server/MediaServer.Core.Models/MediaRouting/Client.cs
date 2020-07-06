using MediaServer.Common.Media;
using MediaServer.WebRtc.Managed;
using System;
using System.Collections.Generic;

namespace MediaServer.Core.Models.MediaRouting
{
    sealed class Client
    {
        public IRemoteDevice Device { get; }

        public Dictionary<MediaQuality, VideoSource> VideoSources { get; } = new Dictionary<MediaQuality, VideoSource>();

        public List<PeerConnection> PeerConnections = new List<PeerConnection>();

        public MediaQuality DesiredMediaQuality => MediaQuality.High; // todo - support multiple quality streams

        public Client(IRemoteDevice device)
        {
            Device = device ?? throw new ArgumentNullException(nameof(device));
        }

        public PeerConnection GetPeerConnectionOrThrow(Guid peerConnectionId)
        {
            PeerConnection peerConnection = null;
            for(var i = 0; i < PeerConnections.Count; i++)
            {
                if(PeerConnections[i].Id == peerConnectionId)
                {
                    peerConnection = PeerConnections[i];
                    break;
                }
            }

            if(null == peerConnection)
                throw new ArgumentException(
                    $"Invalid - {nameof(peerConnectionId)}, " +
                    $"no PeerConnection found with id {peerConnectionId}");
            return peerConnection;
        }

        public override string ToString() => $"[Client {Device.Id.ToString().Substring(0, 8)}]";

    }
}
