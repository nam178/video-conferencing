// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.WebRTC.Tracing;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.MixedReality.WebRTC.Interop
{
    /// <summary>
    /// Interop boolean.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 4)]
    internal struct mrsBool
    {
        public static readonly mrsBool True = new mrsBool(true);
        public static readonly mrsBool False = new mrsBool(false);
        private int _value;
        public mrsBool(bool value) { _value = (value ? -1 : 0); }
        public static explicit operator mrsBool(bool b) { return (b ? True : False); }
        public static explicit operator bool(mrsBool b) { return (b._value != 0); }
    }

    /// <summary>
    /// Attribute to decorate managed delegates used as native callbacks (reverse P/Invoke).
    /// Required by Mono in Ahead-Of-Time (AOT) compiling, and Unity with the IL2CPP backend.
    /// </summary>
    ///
    /// This attribute is required by Mono AOT and Unity IL2CPP, but not by .NET Core or Framework.
    /// The implementation was copied from the Mono source code (https://github.com/mono/mono).
    /// The type argument does not seem to be used anywhere in the code, and a stub implementation
    /// like this seems to be enough for IL2CPP to be able to marshal the delegate (untested on Mono).
    [AttributeUsage(AttributeTargets.Method)]
    sealed class MonoPInvokeCallbackAttribute : Attribute
    {
        public MonoPInvokeCallbackAttribute(Type t) { }
    }

    internal static class Utils
    {
#if MR_SHARING_WIN
        internal const string dllPath = "MediaServer.WebRtc.Native.dll";
#elif MR_SHARING_ANDROID
        internal const string dllPath = "MediaServer.WebRtc.Native.so";
#endif

        // Error codes returned by the interop API -- see mrs_errors.h
        internal const uint MRS_SUCCESS = 0u;
        internal const uint MRS_E_UNKNOWN = 0x80000000u;
        internal const uint MRS_E_INVALID_PARAMETER = 0x80000001u;
        internal const uint MRS_E_INVALID_OPERATION = 0x80000002u;
        internal const uint MRS_E_WRONG_THREAD = 0x80000003u;
        internal const uint MRS_E_NOTFOUND = 0x80000004u;
        internal const uint MRS_E_INVALID_NATIVE_HANDLE = 0x80000005u;
        internal const uint MRS_E_NOT_INITIALIZED = 0x80000006u;
        internal const uint MRS_E_UNSUPPORTED = 0x80000007u;
        internal const uint MRS_E_OUT_OF_RANGE = 0x80000008u;
        internal const uint MRS_E_PEER_CONNECTION_CLOSED = 0x80000101u;
        internal const uint MRS_E_SCTP_NOT_NEGOTIATED = 0x80000301u;
        internal const uint MRS_E_INVALID_DATA_CHANNEL_ID = 0x80000302u;

        public static IntPtr MakeWrapperRef(object obj)
        {
            var handle = GCHandle.Alloc(obj, GCHandleType.Normal);
            var arg = GCHandle.ToIntPtr(handle);
            return arg;
        }

        public static T ToWrapper<T>(IntPtr peer) where T : class
        {
            var handle = GCHandle.FromIntPtr(peer);
            var wrapper = (handle.Target as T);
            return wrapper;
        }

        public static void ReleaseWrapperRef(IntPtr wrapperRef)
        {
            var handle = GCHandle.FromIntPtr(wrapperRef);
            handle.Free();
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        internal struct SdpFilter
        {
            public string CodecName;
            public string ExtraParams;
        }

        [DllImport(dllPath, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi,
            EntryPoint = "mrsReportLiveObjects")]
        public static unsafe extern uint LibraryReportLiveObjects();

        [DllImport(dllPath, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi,
            EntryPoint = "mrsGetShutdownOptions")]
        public static unsafe extern Library.ShutdownOptionsFlags LibraryGetShutdownOptions();

        [DllImport(dllPath, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi,
            EntryPoint = "mrsSetShutdownOptions")]
        public static unsafe extern void LibrarySetShutdownOptions(Library.ShutdownOptionsFlags options);

        [DllImport(dllPath, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi,
            EntryPoint = "mrsForceShutdown")]
        public static unsafe extern void LibraryForceShutdown();

        [DllImport(dllPath, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi,
            EntryPoint = "mrsSdpForceCodecs")]
        public static unsafe extern uint SdpForceCodecs(string message, SdpFilter audioFilter, SdpFilter videoFilter,
            StringBuilder messageOut, ref ulong messageOutLength);


        /// <summary>
        /// Unsafe utility to copy a contiguous block of memory.
        /// This is equivalent to the C function <c>memcpy()</c>, and is provided for optimization purpose only.
        /// </summary>
        /// <param name="dst">Pointer to the beginning of the destination buffer data is copied to.</param>
        /// <param name="src">Pointer to the beginning of the source buffer data is copied from.</param>
        /// <param name="size">Size of the memory block, in bytes.</param>
        [DllImport(dllPath, CallingConvention = CallingConvention.StdCall, EntryPoint = "mrsMemCpy")]
        public static unsafe extern void MemCpy(void* dst, void* src, ulong size);

        /// <summary>
        /// Unsafe utility to copy a memory block with stride.
        ///
        /// This utility loops over the rows of the input memory block, and copy them to the output
        /// memory block, then increment the read and write pointers by the source and destination
        /// strides, respectively. For each row, exactly <paramref name="elem_size"/> bytes are copied,
        /// even if the row stride is higher. The extra bytes in the destination buffer past the row
        /// size until the row stride are left untouched.
        ///
        /// This is equivalent to the following pseudo-code:
        /// <code>
        /// for (int row = 0; row &lt; elem_count; ++row) {
        ///   memcpy(dst, src, elem_size);
        ///   dst += dst_stride;
        ///   src += src_stride;
        /// }
        /// </code>
        /// </summary>
        /// <param name="dst">Pointer to the beginning of the destination buffer data is copied to.</param>
        /// <param name="dst_stride">Stride in bytes of the destination rows. This must be greater than
        /// or equal to the row size <paramref name="elem_size"/>.</param>
        /// <param name="src">Pointer to the beginning of the source buffer data is copied from.</param>
        /// <param name="src_stride">Stride in bytes of the source rows. This must be greater than
        /// or equal to the row size <paramref name="elem_size"/>.</param>
        /// <param name="elem_size">Size of each row, in bytes.</param>
        /// <param name="elem_count">Total number of rows to copy.</param>
        [DllImport(dllPath, CallingConvention = CallingConvention.StdCall, EntryPoint = "mrsMemCpyStride")]
        public static unsafe extern void MemCpyStride(void* dst, int dst_stride, void* src, int src_stride,
            int elem_size, int elem_count);

        /// <summary>
        /// Helper to throw an exception based on an error code.
        /// </summary>
        /// <param name="res">The error code to turn into an exception, if not zero (MRS_SUCCESS).</param>
        public static void ThrowOnErrorCode(uint res)
        {
            if (res == MRS_SUCCESS)
            {
                return;
            }

            MainEventSource.Log.NativeError(res);

            switch (res)
            {
                case MRS_E_UNKNOWN:
                default:
                    throw new Exception();

                case MRS_E_INVALID_PARAMETER:
                    throw new ArgumentException();

                case MRS_E_INVALID_OPERATION:
                    throw new InvalidOperationException();

                case MRS_E_WRONG_THREAD:
                    throw new InvalidOperationException("This method cannot be called on that thread.");

                case MRS_E_NOTFOUND:
                    throw new Exception("Object not found.");

                case MRS_E_INVALID_NATIVE_HANDLE:
                    throw new InvalidInteropNativeHandleException();

                case MRS_E_NOT_INITIALIZED:
                    throw new InvalidOperationException("Object not initialized.");

                case MRS_E_UNSUPPORTED:
                    throw new NotSupportedException();

                case MRS_E_OUT_OF_RANGE:
                    throw new ArgumentOutOfRangeException();

                case MRS_E_SCTP_NOT_NEGOTIATED:
                    throw new SctpNotNegotiatedException();

                case MRS_E_PEER_CONNECTION_CLOSED:
                    throw new InvalidOperationException("The operation cannot complete because the peer connection was closed.");

                case MRS_E_INVALID_DATA_CHANNEL_ID:
                    throw new ArgumentOutOfRangeException("Invalid ID passed to AddDataChannelAsync().");
            }
        }

        /// <summary>
        /// See <see cref="PeerConnection.SetFrameHeightRoundMode(PeerConnection.FrameHeightRoundMode)"/>.
        /// </summary>
        /// <param name="value"></param>
        [DllImport(dllPath, CallingConvention = CallingConvention.StdCall, EntryPoint = "mrsSetFrameHeightRoundMode")]
        public static unsafe extern void SetFrameHeightRoundMode(PeerConnection.FrameHeightRoundMode value);
    }
}
