using MediaServer.Common.Media;
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
        public Guid PeerConnectionId { get; }

        public TransceiverMetadata TransceiverMetadata { get; }

        public TransceiverMetadataUpdatedEvent(Guid peerConnectionId, TransceiverMetadata transceiverMetadata)
        {
            Require.NotEmpty(peerConnectionId);
            Require.NotNull(transceiverMetadata);
            PeerConnectionId = peerConnectionId;
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
        /// After TransceiverMetadataUpdated occurs, 
        /// we send the client the metadata,
        /// then the client will ack that it received the metadata via this method.
        /// </summary>
        void AckTransceiverMetadata(Guid videoClientId, Guid peerConnectionId, string transceiverMid);

        /// <summary>
        /// Notify this router that a remote device has left the current routing.
        /// This is before any data is removed/destroyed, and before any PeerConnection is closed.
        /// </summary>
        /// <remarks>Must be called from signalling thread</remarks>
        void RemoveRemoteDevice(IRemoteDevice remoteDevice);
    }
}