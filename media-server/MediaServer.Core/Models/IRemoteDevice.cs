﻿using MediaServer.WebRtc.Managed;
using System;
using System.Threading.Tasks;

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
        /// Unique id of this device (within the scope of this server)
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Send an user-update message to this device
        /// </summary>
        /// <param name="message"></param>
        /// <exception cref="System.Exception">When network error or things like that occurs</exception>
        /// <returns></returns>
        Task SendSyncMessageAsync(SyncMessage message);

        /// <summary>
        /// Send the
        /// </summary>
        /// <param name="candidate">The ICE candidate genrated from this server</param>
        /// <returns></returns>
        Task SendIceCandidateAsync(RTCIceCandidate candidate);

        /// <summary>
        /// Send the specified SDP to the remote peer
        /// </summary>
        /// <param name="description">Could be either offer or answer</param>
        /// <returns></returns>
        Task SendSessionDescriptionAsync(RTCSessionDescription description);

        /// <summary>
        /// Terminate the connection with this device.
        /// Implementation must not throw exception.
        /// </summary>
        void Teminate();
    }
}