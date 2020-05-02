using MediaServer.WebRtc.Managed;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaServer.WebRtc.MediaRouting
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
            var t = new VideoSource($"{videoClientId.ToString().Substring(0, 8)}-{trackQuality.ToString()}");
            _indexByVideoClientId[videoClientId].VideoSources[trackQuality] = t;
            return t;
        }

        public VideoClient Get(Guid videoClientId)
        {
            ThrowWhenKeyNotExist(videoClientId);
            return _indexByVideoClientId[videoClientId];
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

        public IEnumerable<VideoClient> OtherThan(VideoClient videoClient) => _indexByVideoClientId.Where(kv => kv.Value != videoClient).Select(kv => kv.Value);

        public void Remove(Guid videoClientId)
        {
            if(_indexByVideoClientId.ContainsKey(videoClientId))
            {
                _indexByVideoClientId.Remove(videoClientId);
            }
        }

        void ThrowWhenKeyNotExist(Guid videoClientId)
        {
            if(!_indexByVideoClientId.ContainsKey(videoClientId))
                throw new InvalidOperationException($"VideoClient {videoClientId} has not been added.");
        }
    }
}
