using MediaServer.Common.Media;
using MediaServer.WebRtc.Managed;
using System;
using System.Collections.Generic;

namespace MediaServer.WebRtc.MediaRouting
{
    sealed class VideoClient
    {
        public Guid Id { get; }

        public Dictionary<MediaQuality, VideoSource> VideoSources { get; } = new Dictionary<MediaQuality, VideoSource>();

        public List<PeerConnection> PeerConnections = new List<PeerConnection>();

        public MediaQuality DesiredMediaQuality => MediaQuality.High; // todo - support multiple quality streams

        public VideoClient(Guid id)
        {
            if(id == Guid.Empty)
            {
                throw new ArgumentException();
            }
            Id = id;
        }

        public override string ToString() => $"[VideoClient {Id.ToString().Substring(0, 8)}]";
    }

    static class VideoClientExtensions
    {
        public static PeerConnection GetPeerConnectionOrThrow(this VideoClient videoClient, Guid peerConnectionId)
        {
            PeerConnection peerConnection = null;
            for(var i = 0; i < videoClient.PeerConnections.Count; i++)
            {
                if(videoClient.PeerConnections[i].Id == peerConnectionId)
                {
                    peerConnection = videoClient.PeerConnections[i];
                    break;
                }
            }

            if(null == peerConnection)
                throw new ArgumentException(
                    $"Invalid - {nameof(peerConnectionId)}, " +
                    $"no PeerConnection found with id {peerConnectionId}");
            return peerConnection;
        }
    }
}
