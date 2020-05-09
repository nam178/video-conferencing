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

        public VideoClient AddVideoClient(Guid videoClientId)
        {
            if(_indexByVideoClientId.ContainsKey(videoClientId))
            {
                throw new InvalidOperationException($"VideoClient with id {videoClientId} already added.");
            }
            var videoClient = new VideoClient(videoClientId);
            _indexByVideoClientId[videoClientId] = videoClient;
            return videoClient;
        }

        public VideoSource CreateVideoSource(Guid videoClientId, MediaQuality mediaQuality)
        {
            ThrowWhenKeyNotExist(videoClientId);
            if(_indexByVideoClientId[videoClientId].VideoSources.ContainsKey(mediaQuality))
            {
                throw new InvalidOperationException();
            }
            var t = new VideoSource(_indexByVideoClientId[videoClientId], mediaQuality);
            _indexByVideoClientId[videoClientId].VideoSources[mediaQuality] = t;
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
                    if(kv.Value.PeerConnections[i].Observer == observer)
                    {
                        peerConnection = kv.Value.PeerConnections[i];
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

        public override string ToString() => $"[VideoClientCollection Total Clients={_indexByVideoClientId.Count}]";
    }
}
