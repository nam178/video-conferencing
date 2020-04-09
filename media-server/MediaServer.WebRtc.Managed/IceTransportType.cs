// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Microsoft.MixedReality.WebRTC.Tests")]

namespace Microsoft.MixedReality.WebRTC
{
    /// <summary>
    /// Type of ICE candidates offered to the remote peer.
    /// </summary>
    public enum IceTransportType : int
    {
        /// <summary>
        /// No ICE candidate offered.
        /// </summary>
        None = 0,

        /// <summary>
        /// Only advertize relay-type candidates, like TURN servers, to avoid leaking the IP address of the client.
        /// </summary>
        Relay = 1,

        /// ?
        NoHost = 2,

        /// <summary>
        /// Offer all types of ICE candidates.
        /// </summary>
        All = 3
    }
}
