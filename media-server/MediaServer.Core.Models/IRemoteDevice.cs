using MediaServer.Common.Media;
using MediaServer.Core.Models;
using MediaServer.WebRtc.Common;
using System;
using System.Collections.Generic;

namespace MediaServer.Models
{
    /// <summary>
    /// Represents a remote device that connected to us.
    /// For instance it's a client connected via websocket, however we don't care how they connected,
    /// we see them dumb "devices".
    /// </summary>
    public interface IRemoteDevice
    {
        /// <summary>
        /// Unique id of this device
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Get a copy of custom data associated with this device
        /// </summary>
        /// <returns>The custom data, can be empty but never null</returns>
        RemoteDeviceData GetCustomData();

        /// <summary>
        /// Set custom data associated with this device
        /// </summary>
        /// <remarks>
        /// This method is thread safe however might still race, 
        /// when multiple threads call this, the last one wins.
        /// Therefore should be called from the device message thread only.
        /// </remarks>
        /// <param name="customData"></param>
        void SetCustomData(RemoteDeviceData customData);

        /// <summary>
        /// Send an user-update message to this device
        /// </summary>
        /// <param name="message"></param>
        /// <exception cref="System.Exception">When network error or things like that occurs</exception>
        /// <returns></returns>
        void EnqueueMessage(SyncMessage message);

        /// <summary>
        /// Send the
        /// </summary>
        /// <param name="candidate">The ICE candidate genrated from this server</param>
        /// <returns></returns>
        void EnqueueIceCandidate(Guid peerConnectionId, RTCIceCandidate candidate);

        /// <summary>
        /// Send the specified SDP to the remote peer
        /// </summary>
        /// <param name="description">Could be either offer or answer</param>
        /// <returns></returns>
        void EnqueueAnswer(
            Guid peerConnectionId,
            RTCSessionDescription description,
            IReadOnlyList<TransceiverMetadata> transceivers);

        /// <summary>
        /// Send the specified SDP to the remote peer
        /// </summary>
        /// <param name="description">Could be either offer or answer</param>
        /// <returns></returns>
        void EnqueueOffer(
            Guid peerConnectionId,
            Guid offerId,
            RTCSessionDescription description,
            IReadOnlyList<TransceiverMetadata> transceivers);

        /// <summary>
        /// Terminate the connection with this device.
        /// Implementation must not throw exception.
        /// </summary>
        void Teminate();
    }
}