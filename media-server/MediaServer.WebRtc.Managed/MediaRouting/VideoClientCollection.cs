using MediaServer.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaServer.WebRtc.Managed.MediaRouting
{
    sealed class VideoClientCollection
    {
        readonly Dictionary<Guid, VideoClient> _indexByVideoClientId;

        public VideoClientCollection()
        {
            _indexByVideoClientId = new Dictionary<Guid, VideoClient>();
        }

        public void AddVideoClient(Guid videoClientId)
        {
            if(_indexByVideoClientId.ContainsKey(videoClientId))
            {
                throw new InvalidOperationException($"VideoClient with id {videoClientId} already added.");
            }
            _indexByVideoClientId[videoClientId] = new VideoClient(videoClientId);
        }

        public VideoSource CreateVideoSource(Guid videoClientId, TrackQuality trackQuality)
        {
            ThrowWhenKeyNotExist(videoClientId);
            if(_indexByVideoClientId[videoClientId].VideoSources.ContainsKey(trackQuality))
            {
                throw new InvalidOperationException();
            }
            var t = new VideoSource();
            _indexByVideoClientId[videoClientId].VideoSources[trackQuality] = t;
            return t;
        }

        public VideoClient Get(Guid videoClientId)
        {
            ThrowWhenKeyNotExist(videoClientId);
            return _indexByVideoClientId[videoClientId];
        }

        public VideoSource FindVideoSource(Guid videoClientId, TrackQuality trackQuality)
        {
            ThrowWhenKeyNotExist(videoClientId);
            return _indexByVideoClientId[videoClientId].VideoSources.ContainsKey(trackQuality)
                ? _indexByVideoClientId[videoClientId].VideoSources[trackQuality]
                : null;
        }

        public TrackQuality? FindQuality(Guid devieId, string trackId)
        {
            Require.NotNullOrWhiteSpace(trackId);
            ThrowWhenKeyNotExist(devieId);

            foreach(var kv in _indexByVideoClientId[devieId].VideoSources)
            {
                if(string.Equals(kv.Value.ExpectedTrackId, trackId, StringComparison.InvariantCultureIgnoreCase))
                {
                    return kv.Key;
                }
            }
            return null;
        }

        public VideoClient FindByObserver(PeerConnectionObserver observer, out PeerConnection peerConnection)
        {
            foreach(var kv in _indexByVideoClientId)
            {
                for(var i = 0; i < kv.Value.PeerConnections.Count; i++)
                {
                    if(kv.Value.PeerConnections[i].PeerConnectionObserver == observer)
                    {
                        peerConnection = kv.Value.PeerConnections[i].PeerConnection;
                        return kv.Value;
                    }
                }
            }
            peerConnection = null;
            return null;
        }

        void ThrowWhenKeyNotExist(Guid videoClientId)
        {
            if(!_indexByVideoClientId.ContainsKey(videoClientId))
                throw new InvalidOperationException($"VideoClient {videoClientId} has not been added.");
        }

        public IEnumerable<VideoClient> OtherThan(VideoClient videoClient) => _indexByVideoClientId.Where(kv => kv.Value != videoClient).Select(kv => kv.Value);
    }
}
