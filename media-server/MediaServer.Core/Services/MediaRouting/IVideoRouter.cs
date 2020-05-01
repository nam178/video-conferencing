using MediaServer.WebRtc.Managed;
using System;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.MediaRouting
{
    public interface IVideoRouter
    {
        /// <summary>
        /// Notify this router that a PeerConnection is created.
        /// Must be called right after the PeerConnection is created, before SetRemoteSessionDescription()
        /// </summary>
        /// <param name="videoClientId"></param>
        /// <param name="peerConnection"></param>
        /// <remarks>Can be called on any thread</remarks>
        Task AddPeerConnection(
            Guid videoClientId,
            WebRtc.Managed.PeerConnection peerConnection,
            PeerConnectionObserver peerConnectionObserver);

        /// <summary>
        /// Notify this router that a video client has joined the routing.
        /// This is before any PeerConnection with the video client is created.
        /// </summary>
        /// <param name="videoClientId"></param>
        /// <remarks>Can be called from any thread, will dispatch to the signalling thread</remarks>
        /// <returns></returns>
        Task AddVideoClient(Guid videoClientId);

        /// <summary>
        /// Notify this router that a track will be added into it with specified quality.
        /// </summary>
        /// <param name="videoClientId">The video client in which the track will be added</param>
        /// <param name="trackQuality"></param>
        /// <param name="trackId"></param>
        /// <remarks>Can be called from any thread, will be switched to the signalling thread.</remarks>
        /// <returns></returns>
        Task PepareTrack(Guid videoClientId, TrackQuality trackQuality, string trackId);

        /// <summary>
        /// Notify this router that a PeerConnection will be removed/closed.
        /// Must be called right before the PeerConnection is closed.
        /// </summary>
        /// <param name="videoClientId"></param>
        /// <param name="peerConnection"></param>
        /// <remarks>Can be called on any thread</remarks>
        Task RemovePeerConnection(
            Guid videoClientId,
            WebRtc.Managed.PeerConnection peerConnection,
            PeerConnectionObserver peerConnectionObserver);

        /// <summary>
        /// Notify this router that a video client has left the current routing.
        /// This is before any data is removed/destroyed, and before any PeerConnection is closed.
        /// </summary>
        /// <param name="videoClientId"></param>
        /// <remarks>Can be called from any thread</remarks>
        /// <returns></returns>
        Task RemoveVideoClient(Guid videoClientId);
    }
}