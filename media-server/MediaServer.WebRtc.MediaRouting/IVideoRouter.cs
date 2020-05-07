﻿using MediaServer.WebRtc.Managed;
using System;
using System.Threading.Tasks;

namespace MediaServer.WebRtc.MediaRouting
{
    public interface IVideoRouter
    {
        /// <summary>
        /// Notify this router that a video client has joined the routing.
        /// This is before any PeerConnection with the video client is created.
        /// </summary>
        /// <param name="videoClientId"></param>
        /// <remarks>Can be called from any thread, will dispatch to the signalling thread</remarks>
        Task AddVideoClientAsync(Guid videoClientId);

        /// <summary>
        /// Notify this router that a track will be added into it with specified quality.
        /// </summary>
        /// <param name="videoClientId">The video client in which the track will be added</param>
        /// <remarks>Can be called from any thread, will be switched to the signalling thread.</remarks>
        Task Prepare(Guid videoClientId, MediaQuality trackQuality, MediaKind kind, string trackId);

        /// <summary>
        /// Notify this router that a video client has left the current routing.
        /// This is before any data is removed/destroyed, and before any PeerConnection is closed.
        /// </summary>
        /// <param name="videoClientId"></param>
        /// <remarks>Can be called from any thread</remarks>
        Task RemoveVideoClientAsync(Guid videoClientId);
    }
}