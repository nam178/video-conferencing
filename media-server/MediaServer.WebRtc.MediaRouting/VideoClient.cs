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

        public MediaQuality DesiredVideoQuality => MediaQuality.High; // todo - support multiple quality streams

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
}
