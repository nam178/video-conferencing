using MediaServer.WebRtc.Managed;
using System;
using System.Collections.Generic;

namespace MediaServer.Core.Services.MediaRouting
{
    /// <summary>
    /// Manages the zero-to-one relationship between VideoSource -> RtpReceiver.
    /// One VideoSource can receive video from max 1 RtpReceiver.
    /// </summary>
    /// <remarks>Not thread safe</remarks>
    sealed class RemoteVideoLinkCollection
    {
        readonly Dictionary<RtpReceiver, VideoSource> _idx_trackToSource = new Dictionary<RtpReceiver, VideoSource>();
        readonly Dictionary<VideoSource, RtpReceiver> _idx_sourceToTrack = new Dictionary<VideoSource, RtpReceiver>();

        public void AddOrUpdate(RemoteVideoLink link)
        {
            if(link is null)
                throw new ArgumentNullException(nameof(link));

            // Updating track's source? Not allowed.
            if(_idx_trackToSource.ContainsKey(link.RemoteTrack))
                throw new InvalidProgramException("A track is not permitted to change soure");

            // Updating source's track? Make sure we remove the old one first.
            if(_idx_sourceToTrack.ContainsKey(link.VideoSource))
                RemoveByVideoSource(link.VideoSource);

            // Then connect the new one
            Connect(link.RemoteTrack, link.VideoSource);

            // Update the index
            _idx_sourceToTrack[link.VideoSource] = link.RemoteTrack;
            _idx_trackToSource[link.RemoteTrack] = link.VideoSource;
        }

        public void RemoveByRemoteTrack(RtpReceiver remoteTrack)
        {
            if(remoteTrack is null)
                throw new ArgumentNullException(nameof(remoteTrack));
            if(false == _idx_trackToSource.ContainsKey(remoteTrack))
                return;

            var source = _idx_trackToSource[remoteTrack];
            if(null == source)
                throw new NullReferenceException(nameof(source));
            Disconnect(remoteTrack, source);

            _idx_sourceToTrack.Remove(source);
            _idx_trackToSource.Remove(remoteTrack);
        }

        public void RemoveByVideoSource(VideoSource videoSource)
        {
            if(videoSource is null)
                throw new ArgumentNullException(nameof(videoSource));
            if(false == _idx_sourceToTrack.ContainsKey(videoSource))
                return;

            var track = _idx_sourceToTrack[videoSource];
            if(false == _idx_sourceToTrack.Remove(videoSource))
                throw new InvalidProgramException("Failed removing VideoSource");
            _idx_trackToSource.Remove(track);

            Disconnect(track, videoSource);
        }

        static void Disconnect(RtpReceiver remoteTrack, VideoSource source)
            => ((VideoTrack)remoteTrack.Track).RemoveSink(source.VideoSinkAdapter);

        static void Connect(RtpReceiver remoteTrack, VideoSource videoSource)
            => ((VideoTrack)remoteTrack.Track).AddSink(videoSource.VideoSinkAdapter);
    }
}
