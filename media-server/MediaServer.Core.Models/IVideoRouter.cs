﻿using MediaServer.Common.Media;
using System;
using System.Collections.Generic;

namespace MediaServer.Core.Models
{
    /// <summary>
    /// Media router deals with the complicated add/remove tracks, transceivers, etc.
    /// All we need to do is to tell us about those involve (peer connections, devices, etc.);
    /// </summary>
    public interface IVideoRouter
    {
        /// <summary>
        /// Notify this router that a video client has joined the routing.
        /// This is before any PeerConnection with the video client is created.
        /// </summary>
        /// <param name="videoClientId"></param>
        /// <remarks>Must be called from signalling thread</remarks>
        void AddVideoClient(Guid videoClientId);

        /// <summary>
        /// Notify this router that we received remote transceiver metadata.
        /// </summary>
        /// <param name="videoClientId">The video client in which the track will be added</param>
        /// <remarks>Must be called from signalling thread</remarks>
        void SetRemoteTransceiverMetadata(TransceiverMetadata transceiverMetadata);

        /// <summary>
        /// Clients can use this to generate transceiver metadata to send along with SDP.
        /// </summary>
        /// <param name="videoClientId"></param>
        /// <param name="peerConnectionId"></param>
        /// <returns>Transceiver metadata as a copy</returns>
        /// <remarks>Must be called from signalling thread</remarks>
        IReadOnlyList<TransceiverMetadata> GetLocalTransceiverMetadata(Guid videoClientId, Guid peerConnectionId);

        /// <summary>
        /// Notify the router that it's clear to activate frozen transceivers, 
        /// Use this when the remote peer has ack'ed our latest transceiver metadata. 
        /// (via sdp answer message)
        /// 
        /// Ununsed transceivers are frozen, that is, marked not suitable for reuse,
        /// until the remote client has acked that they know about those unused transceivers,
        /// (so they can remove from their UIs);
        /// 
        /// If we don't do this, since re-using transceivers are seamless, UI clients may 
        /// (briefly) display people in the wrong spot when we switch transceivers'
        /// tracks.
        /// </summary>
        void ClearFrozenTransceivers(Guid videoClientId, Guid peerConnectionId);

        /// <summary>
        /// Notify this router that a video client has left the current routing.
        /// This is before any data is removed/destroyed, and before any PeerConnection is closed.
        /// </summary>
        /// <param name="videoClientId"></param>
        /// <remarks>Must be called from signalling thread</remarks>
        void RemoveVideoClient(Guid videoClientId);
    }
}