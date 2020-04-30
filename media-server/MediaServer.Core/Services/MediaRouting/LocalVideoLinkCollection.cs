using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaServer.Core.Services.MediaRouting
{
    /// <summary>
    /// Represents many-to-many relationshop between PeerConnection and VideoSource.
    /// One PeerConnection can has many VideoSource (video streams from other peers)
    /// </summary>
    /// <remarks>Not thread safe.</remarks>
    sealed class LocalVideoLinkCollection
    {
        readonly Dictionary<WebRtc.Managed.PeerConnection, List<LocalVideoLink>> _idx_peerConnection;
        readonly Dictionary<VideoSource, List<LocalVideoLink>> _idx_videoSource;

        public LocalVideoLinkCollection()
        {
            _idx_peerConnection = new Dictionary<WebRtc.Managed.PeerConnection, List<LocalVideoLink>>();
            _idx_videoSource = new Dictionary<VideoSource, List<LocalVideoLink>>();
        }

        public void Add(LocalVideoLink link)
        {
            Add(_idx_videoSource, link.VideoSource, link);
            Add(_idx_peerConnection, link.TargetPeerConnection, link);
        }

        public void RemoveByPeerConnection(WebRtc.Managed.PeerConnection peerConnection)
        {
            if(_idx_peerConnection.ContainsKey(peerConnection))
            {
                foreach(var link in _idx_peerConnection[peerConnection].ToList())
                {
                    if(false == _idx_videoSource.ContainsKey(link.VideoSource))
                        throw new InvalidProgramException();

                    using(link)
                    {
                        Remove(_idx_videoSource, link.VideoSource, link);
                        link.Close();
                    }
                }

                _idx_peerConnection.Remove(peerConnection);
            }
        }

        public void RemoveByVideoSource(VideoSource videoSource)
        {
            if(_idx_videoSource.ContainsKey(videoSource))
            {
                foreach(var link in _idx_videoSource[videoSource].ToList())
                {
                    if(false == _idx_peerConnection.ContainsKey(link.TargetPeerConnection))
                        throw new InvalidProgramException();

                    using(link)
                    {
                        Remove(_idx_peerConnection, link.TargetPeerConnection, link);
                        link.Close();
                    }
                }

                _idx_videoSource.Remove(videoSource);
            }
        }

        static void Add<K, V>(Dictionary<K, List<V>> dict, K key, V value)
        {
            if(false == dict.ContainsKey(key))
            {
                dict[key] = new List<V> { value };
            }
            else
            {
                if(dict[key].Contains(value))
                {
                    throw new ArgumentException($"Duplicate value for {typeof(K).Name}={key}, {typeof(V).Name}={value}");
                }
                dict[key].Add(value);
            }
        }

        static void Remove<K, V>(Dictionary<K, List<V>> dict, K key, V value)
        {
            if(false == dict.ContainsKey(key))
                return;

            dict[key].Remove(value);
            if(dict[key].Count == 0)
            {
                dict.Remove(key);
            }
        }

    }
}
