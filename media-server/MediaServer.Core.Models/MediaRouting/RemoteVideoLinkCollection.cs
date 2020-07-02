using MediaServer.WebRtc.Managed;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaServer.Core.Models.MediaRouting
{
    /// <summary>
    /// Manages the zero-to-one relationship between VideoSource -> RtpReceiver.
    /// One VideoSource can receive video from max 1 RtpReceiver.
    /// </summary>
    /// <remarks>Not thread safe</remarks>
    sealed class RemoteVideoLinkCollection
    {
        readonly Dictionary<RtpReceiver, (VideoSource Source, PeerConnection PeerConnection)> _indexByVideoTrack;
        readonly Dictionary<VideoSource, RtpReceiver> _indexByVideoSource;
        readonly Dictionary<PeerConnection, HashSet<RtpReceiver>> _indexByPeerConnection;

        public RemoteVideoLinkCollection()
        {
            _indexByVideoTrack = new Dictionary<RtpReceiver, (VideoSource Sorce, PeerConnection PeerConnection)>();
            _indexByVideoSource = new Dictionary<VideoSource, RtpReceiver>();
            _indexByPeerConnection = new Dictionary<PeerConnection, HashSet<RtpReceiver>>();
        }

        public void AddOrUpdate(RemoteVideoLink link)
        {
            if(link is null)
                throw new ArgumentNullException(nameof(link));

            // Updating track's source? Not allowed.
            if(_indexByVideoTrack.ContainsKey(link.RemoteTrack))
                throw new InvalidProgramException("A track is not permitted to change soure");

            // Updating source's track? Make sure we remove the old one first.
            if(_indexByVideoSource.ContainsKey(link.VideoSource))
                RemoveByVideoSource(link.VideoSource);

            // Then connect the new one
            Connect(link.RemoteTrack, link.VideoSource);

            // Update the index
            _indexByVideoSource[link.VideoSource] = link.RemoteTrack;
            _indexByVideoTrack[link.RemoteTrack] = (link.VideoSource, link.PeerConnection);
            _indexByPeerConnection.Add(link.PeerConnection, link.RemoteTrack);
        }

        public void RemoveByRemoteTrack(RtpReceiver remoteTrack)
        {
            if(remoteTrack is null)
                throw new ArgumentNullException(nameof(remoteTrack));
            if(false == _indexByVideoTrack.ContainsKey(remoteTrack))
                return;

            var link = _indexByVideoTrack[remoteTrack];
            Disconnect(remoteTrack, link.Source);

            _indexByVideoSource.Remove(link.Source);
            _indexByVideoTrack.Remove(remoteTrack);
            _indexByPeerConnection.Remove(link.PeerConnection, remoteTrack);
        }

        public bool Exists(VideoSource videoSource) => _indexByVideoSource.ContainsKey(videoSource);

        public void RemoveByPeerConnection(PeerConnection peerConnection)
        {
            if(_indexByPeerConnection.ContainsKey(peerConnection))
            {
                foreach(var track in _indexByPeerConnection[peerConnection].ToList())
                {
                    RemoveByRemoteTrack(track);
                }
            }
        }

        static void Disconnect(RtpReceiver remoteTrack, VideoSource source)
            => ((VideoTrack)remoteTrack.Track).RemoveSink(source.VideoSinkAdapter);

        static void Connect(RtpReceiver remoteTrack, VideoSource videoSource)
            => ((VideoTrack)remoteTrack.Track).AddSink(videoSource.VideoSinkAdapter);

        void RemoveByVideoSource(VideoSource videoSource)
        {
            if(videoSource is null)
                throw new ArgumentNullException(nameof(videoSource));
            if(false == _indexByVideoSource.ContainsKey(videoSource))
                return;

            // Remove VideoSource omdex
            var track = _indexByVideoSource[videoSource];
            if(false == _indexByVideoSource.Remove(videoSource))
                throw new InvalidProgramException("Failed removing VideoSource");

            // Remote track index
            if(false == _indexByVideoTrack.ContainsKey(track))
                throw new InvalidProgramException("Failed removing VideoSource");
            var link = _indexByVideoTrack[track];
            _indexByVideoTrack.Remove(track);

            // Remove PeerConnection index
            _indexByPeerConnection.Remove(link.PeerConnection, track);

            Disconnect(track, videoSource);
        }

        public override string ToString() => $"[RemoteVideoLinkCollection Total Tracks={_indexByVideoTrack.Count}, Total Sources={_indexByVideoSource.Count}]";
    }
}
