using System;
using System.Collections.Generic;

namespace MediaServer.WebRtc.Managed.MediaRouting
{
    sealed class VideoClient
    {
        public Guid Id { get; }

        public Dictionary<TrackQuality, VideoSource> VideoSources { get; } = new Dictionary<TrackQuality, VideoSource>();

        public List<PeerConnectionEntry> PeerConnections = new List<PeerConnectionEntry>();

        public TrackQuality DesiredRemoteQuality => TrackQuality.High; // todo - support multiple quality streams

        public bool IsPrimaryPeerConnection(PeerConnection peerConnection)
        {
            // The primary PeerConnection simply 
            // the first one added for this device
            return PeerConnections.Count > 0 && PeerConnections[0].PeerConnection == peerConnection;
        }

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
