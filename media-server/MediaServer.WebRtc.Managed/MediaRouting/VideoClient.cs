using System;
using System.Collections.Generic;

namespace MediaServer.WebRtc.Managed.MediaRouting
{
    sealed class VideoClient
    {
        public Guid Id { get; }

        public Dictionary<TrackQuality, VideoSource> VideoSources { get; } = new Dictionary<TrackQuality, VideoSource>();

        public List<PeerConnectionEntry> PeerConnections = new List<PeerConnectionEntry>();

        public VideoClient(Guid id)
        {
            if(id == Guid.Empty)
            {
                throw new ArgumentException();
            }
            Id = id;
        }
    }
}
