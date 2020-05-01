using MediaServer.Common.Utils;
using MediaServer.WebRtc.Managed;
using NLog;
using System;

namespace MediaServer.WebRtc.MediaRouting
{
    sealed class VideoRouterEventHandler
    {
        readonly VideoRouter _videoRouter;
        readonly VideoClientCollection _videoClients;
        readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public VideoRouterEventHandler(
            VideoRouter videoRouter,
            VideoClientCollection videoClients)
        {
            _videoRouter = videoRouter;
            _videoClients = videoClients;
        }

        public void RemoteTrackAdded(object sender, EventArgs<RtpReceiver> e)
        {
            var observer = (PeerConnectionObserver)sender;
            var videoClient = _videoClients.FindByObserver(observer, out var peerConnection);
            if(null == videoClient)
            {
                _logger.Error($"VideoClient not found for observer with track={e.Payload}");
                return;
            }

            try
            {
                _videoRouter.AddRemoteTrack(videoClient, e.Payload);
            }
            catch(Exception ex)
            {
                _logger.Error(ex, "Failed adding remote track into VideoRouter");
            }
        }

        public void RemoteTrackRemoved(object sender, EventArgs<RtpReceiver> e)
        {
            var observer = (PeerConnectionObserver)sender;
            var videoClient = _videoClients.FindByObserver(observer, out var peerConnection);
            if(null == videoClient)
            {
                _logger.Error($"VideoClient not found for observer with track={e.Payload}");
                return;
            }

            try
            {
                _videoRouter.RemoveRemoteTrack(e.Payload);
            }
            catch(Exception ex)
            {
                _logger.Error(ex, "Failed removing remote track from VideoRouter");
            }
        }
    }
}