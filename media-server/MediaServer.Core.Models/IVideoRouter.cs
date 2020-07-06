﻿using MediaServer.Common.Media;
using MediaServer.Common.Utils;
using System;
using System.Collections.Generic;

namespace MediaServer.Core.Models
{
    /// <summary>
    /// Occurs when a transceiver's content has changed, the supplied metadata
    /// will need to be sent to the coresponding client.
    /// </summary>
    public sealed class TransceiverMetadataUpdatedEvent
    {
        public IPeerConnection PeerConnection { get; }

        public TransceiverMetadata TransceiverMetadata { get; }

        public TransceiverMetadataUpdatedEvent(IPeerConnection peerConnection, TransceiverMetadata transceiverMetadata)
        {
            Require.NotNull(transceiverMetadata);
            PeerConnection = peerConnection;
            TransceiverMetadata = transceiverMetadata;
        }
    }

    /// <summary>
    /// Media router deals with the complicated add/remove tracks, transceivers, etc.
    /// All we need to do is to tell us about those involve (peer connections, devices, etc.);
    /// </summary>
    public interface IVideoRouter : IObservable<TransceiverMetadataUpdatedEvent>
    {
        /// <summary>
        /// Notify this router that a remote device has joined the routing.
        /// This is before any PeerConnection with the remote device is created.
        /// </summary>
        /// <remarks>Must be called from signalling thread</remarks>
        void AddRemoteDevice(IRemoteDevice remoteDevice);

        /// <summary>
        /// Notify this router that a remote device has left the current routing.
        /// This is before any data is removed/destroyed, and before any PeerConnection is closed.
        /// </summary>
        /// <remarks>Must be called from signalling thread</remarks>
        void RemoveRemoteDevice(IRemoteDevice remoteDevice);

        /// <summary>
        /// Notify this router that a PeerConnection has been created for the specified device
        /// </summary>
        void AddPeerConnection(IRemoteDevice remote, IPeerConnection peerConnection);

        /// <summary>
        /// Notify this router that a PeerConnection has been closed for the specified device
        /// </summary>
        void RemovePeerConnection(IRemoteDevice remoteDevice, IPeerConnection peerConnection);

        /// <summary>
        /// Notify this router that we received remote transceiver metadata.
        /// </summary>
        /// <param name="videoClientId">The video client in which the track will be added</param>
        /// <remarks>Must be called from signalling thread</remarks>
        void SetRemoteTransceiverMetadata(TransceiverMetadata transceiverMetadata);

        /// <summary>
        /// Clients can use this to generate transceiver metadata to send along with SDP.
        /// </summary>
        /// <returns>Transceiver metadata as a copy</returns>
        /// <remarks>Must be called from signalling thread</remarks>
        IReadOnlyList<TransceiverMetadata> GetLocalTransceiverMetadata(IPeerConnection peerConnection);

        /// <summary>
        /// After TransceiverMetadataUpdated occurs, 
        /// we send the client the metadata,
        /// then the client will ack that it received the metadata via this method.
        /// </summary>
        void AckTransceiverMetadata(IPeerConnection peerConnection, string transceiverMid);
    }
}