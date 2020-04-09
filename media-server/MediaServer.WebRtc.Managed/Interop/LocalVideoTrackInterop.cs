// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.MixedReality.WebRTC.Interop
{
    /// <summary>
    /// Handle to a native local video track object.
    /// </summary>
    internal sealed class LocalVideoTrackHandle : SafeHandle
    {
        /// <summary>
        /// Check if the current handle is invalid, which means it is not referencing
        /// an actual native object. Note that a valid handle only means that the internal
        /// handle references a native object, but does not guarantee that the native
        /// object is still accessible. It is only safe to access the native object if
        /// the handle is not closed, which implies it being valid.
        /// </summary>
        public override bool IsInvalid
        {
            get
            {
                return (handle == IntPtr.Zero);
            }
        }

        /// <summary>
        /// Default constructor for an invalid handle.
        /// </summary>
        public LocalVideoTrackHandle() : base(IntPtr.Zero, ownsHandle: true)
        {
        }

        /// <summary>
        /// Constructor for a valid handle referencing the given native object.
        /// </summary>
        /// <param name="handle">The valid internal handle to the native object.</param>
        public LocalVideoTrackHandle(IntPtr handle) : base(IntPtr.Zero, ownsHandle: true)
        {
            SetHandle(handle);
        }

        /// <summary>
        /// Release the native object while the handle is being closed.
        /// </summary>
        /// <returns>Return <c>true</c> if the native object was successfully released.</returns>
        protected override bool ReleaseHandle()
        {
            LocalVideoTrackInterop.LocalVideoTrack_RemoveRef(handle);
            return true;
        }
    }

    internal class LocalVideoTrackInterop
    {
        #region Native callbacks

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate void VideoFrameUnmanagedCallback(IntPtr userData, IntPtr frame);

        #endregion


        #region Native functions

        [DllImport(Utils.dllPath, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi,
            EntryPoint = "mrsLocalVideoTrackAddRef")]
        public static unsafe extern void LocalVideoTrack_AddRef(LocalVideoTrackHandle handle);

        // Note - This is used during SafeHandle.ReleaseHandle(), so cannot use LocalVideoTrackHandle
        [DllImport(Utils.dllPath, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi,
            EntryPoint = "mrsLocalVideoTrackRemoveRef")]
        public static unsafe extern void LocalVideoTrack_RemoveRef(IntPtr handle);

        [DllImport(Utils.dllPath, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi,
            EntryPoint = "mrsLocalVideoTrackIsEnabled")]
        public static extern int LocalVideoTrack_IsEnabled(LocalVideoTrackHandle trackHandle);

        [DllImport(Utils.dllPath, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi,
            EntryPoint = "mrsLocalVideoTrackSetEnabled")]
        public static extern uint LocalVideoTrack_SetEnabled(LocalVideoTrackHandle trackHandle, int enabled);

        #endregion

        public class InteropCallbackArgs
        {
            public LocalVideoTrack Track;
        }
    }
}
