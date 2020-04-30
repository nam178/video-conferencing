using MediaServer.Common.Utils;
using NLog;

namespace MediaServer.WebRtc.Managed.MediaRouting
{
    sealed class VideoRouterEventHandler
    {
        readonly VideoRouter _videoRouter;
        readonly VideoClientCollection _videoClients;
        readonly ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

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

            _videoRouter.AddRemoteTrack(videoClient, peerConnection, e.Payload);
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

            _videoRouter.RemoveRemoteTrack(videoClient, peerConnection, e.Payload);
        }
    }
}