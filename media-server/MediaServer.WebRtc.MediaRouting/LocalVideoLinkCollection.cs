using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaServer.WebRtc.MediaRouting
{
    /// <summary>
    /// Represents many-to-many relationshop between PeerConnection and VideoSource.
    /// One PeerConnection can has many VideoSource (video streams from other peers)
    /// </summary>
    /// <remarks>Not thread safe.</remarks>
    sealed class LocalVideoLinkCollection
    {
        readonly Dictionary<Managed.PeerConnection, HashSet<LocalVideoLink>> _indexByPeerConnection;
        readonly Dictionary<VideoSource, HashSet<LocalVideoLink>> _indexByVideoSource;

        public LocalVideoLinkCollection()
        {
            _indexByPeerConnection = new Dictionary<Managed.PeerConnection, HashSet<LocalVideoLink>>();
            _indexByVideoSource = new Dictionary<VideoSource, HashSet<LocalVideoLink>>();
        }

        public void Add(LocalVideoLink link)
        {
            _indexByVideoSource.Add(link.VideoSource, link);
            _indexByPeerConnection.Add(link.TargetPeerConnection, link);
        }

        public void RemoveByPeerConnection(Managed.PeerConnection peerConnection)
        {
            if(_indexByPeerConnection.ContainsKey(peerConnection))
            {
                var links = _indexByPeerConnection[peerConnection].ToList();
                for(int i = 0; i < links.Count; i++)
                {
                    var link = links[i];
                    if(false == _indexByVideoSource.ContainsKey(link.VideoSource))
                        throw new InvalidProgramException();

                    using(link)
                    {
                        _indexByVideoSource.Remove(link.VideoSource, link);
                        link.Close();
                    }
                }
                _indexByPeerConnection.Remove(peerConnection);
            }
        }

        public void RemoveByVideoSource(VideoSource videoSource)
        {
            if(_indexByVideoSource.ContainsKey(videoSource))
            {
                var links = _indexByVideoSource[videoSource].ToList();
                for(int i = 0; i < links.Count; i++)
                {
                    var link = links[i];
                    if(false == _indexByPeerConnection.ContainsKey(link.TargetPeerConnection))
                        throw new InvalidProgramException();

                    using(link)
                    {
                        _indexByPeerConnection.Remove(link.TargetPeerConnection, link);
                        link.Close();
                    }
                }

                _indexByVideoSource.Remove(videoSource);
            }
        }

        public override string ToString() => $"[LocalVideoLinkCollection Total PeerConnections={_indexByPeerConnection.Count}, Total VideoSources={_indexByVideoSource.Count}]";
    }
}
