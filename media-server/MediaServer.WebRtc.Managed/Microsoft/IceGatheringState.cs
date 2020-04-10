// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Microsoft.MixedReality.WebRTC.Tests")]

namespace Microsoft.MixedReality.WebRTC
{
    /// <summary>
    /// State of an ICE gathering process.
    /// </summary>
    /// <remarks>
    /// See <see href="https://www.w3.org/TR/webrtc/#rtcicegatheringstate-enum">RTPIceGatheringState</see>
    /// from the WebRTC 1.0 standard.
    /// </remarks>
    /// <seealso href="https://www.w3.org/TR/webrtc/#rtcicegatheringstate-enum"/>
    public enum IceGatheringState : int
    {
        /// <summary>
        /// There is no ICE transport, or none of them started gathering ICE candidates.
        /// </summary>
        New = 0,

        /// <summary>
        /// The gathering process started. At least one ICE transport is active and gathering
        /// some ICE candidates.
        /// </summary>
        Gathering = 1,

        /// <summary>
        /// The gathering process is complete. At least one ICE transport was active, and
        /// all transports finished gathering ICE candidates.
        /// </summary>
        Complete = 2,
    }
}
