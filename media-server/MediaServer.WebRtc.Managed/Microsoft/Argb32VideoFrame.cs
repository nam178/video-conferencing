// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.MixedReality.WebRTC
{
    /// <summary>
    /// Single video frame encoded in ARGB interleaved format (32 bits per pixel).
    /// 
    /// The ARGB components are in the order of a little endian 32-bit integer, so
    /// 0xAARRGGBB, or (B, G, R, A) as a sequence of bytes in memory with B first
    /// and A last.
    /// </summary>
    /// <remarks>
    /// The use of <c>ref struct</c> is an optimization to avoid heap allocation on each frame while
    /// having a nicer-to-use container to pass a frame accross methods.
    /// </remarks>
    public ref struct Argb32VideoFrame
    {
        /// <summary>
        /// Frame width, in pixels.
        /// </summary>
        public uint width;

        /// <summary>
        /// Frame height, in pixels.
        /// </summary>
        public uint height;

        /// <summary>
        /// Pointer to the data buffer containing the ARBG data for each pixel.
        /// </summary>
        public IntPtr data;

        /// <summary>
        /// Stride in bytes between the ARGB rows.
        /// </summary>
        public int stride;
    }
}
