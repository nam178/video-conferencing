using MediaServer.Common.Threading;
using MediaServer.Common.Utils;
using MediaServer.WebRtc.Managed;
using NLog;
using System;

namespace MediaServer.WebRtc.MediaRouting
{
    sealed class VideoRouterEventHandler
    {
        readonly IThread _signallingThread;
        readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        readonly VideoRouter _videoRouter;
        readonly VideoClientCollection _videoClients;

        public VideoRouterEventHandler(
            IThread signallingThread,
            VideoRouter videoRouter,
            VideoClientCollection videoClients)
        {
            _signallingThread = signallingThread ?? throw new ArgumentNullException(nameof(signallingThread));
            _videoRouter = videoRouter;
            _videoClients = videoClients;
        }

        public void RemoteTrackAdded(object sender, EventArgs<RtpTransceiver> e) // signalling thread
        {
            _signallingThread.EnsureCurrentThread();

            var observer = (PeerConnectionObserver)sender;
            var videoClient = _videoClients.FindByObserver(observer, out var peerConnection);
            if(null == videoClient)
            {
                _logger.Error($"VideoClient not found for observer with track={e.Payload}");
                return;
            }

            var track = e.Payload.Receiver.Track;
            if(null == track)
                throw new NullReferenceException(nameof(track));

            // TODO: handle audio tracks
            if(track.Kind == MediaKind.Audio)
            {
                _logger.Warn("Ignored an audio track");
                return;
            }

            try
            {
                _videoRouter.OnRemoteTrackAdded(videoClient, peerConnection, e.Payload);
            }
            catch(Exception ex)
            {
                _logger.Error(ex, "Failed adding remote track into VideoRouter");
            }
        }

        public void RemoteTrackRemoved(object sender, EventArgs<RtpTransceiver> e)
        {
            _signallingThread.EnsureCurrentThread();

            var observer = (PeerConnectionObserver)sender;
            var videoClient = _videoClients.FindByObserver(observer, out var peerConnection);
            if(null == videoClient)
            {
                _logger.Error($"VideoClient not found for observer with track={e.Payload}");
                return;
            }

            var track = e.Payload.Receiver.Track;
            if(null == track)
                throw new NullReferenceException(nameof(track));

            // TODO: handle audio tracks
            if(track.Kind == MediaKind.Audio)
            {
                _logger.Warn("Ignored an audio track");
                return;
            }

            try
            {
                _videoRouter.OnRemoteTrackRemoved(e.Payload);
            }
            catch(Exception ex)
            {
                _logger.Error(ex, "Failed removing remote track from VideoRouter");
            }
        }
    }
}