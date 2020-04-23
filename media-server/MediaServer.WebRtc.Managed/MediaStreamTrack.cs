using System;
using System.Runtime.InteropServices;

namespace MediaServer.WebRtc.Managed
{
    sealed class MediaStreamTrackSafeHandle : SafeHandle
    {
        public MediaStreamTrackSafeHandle(IntPtr native) : base(IntPtr.Zero, true)
        {
            if(native == IntPtr.Zero)
                throw new ArgumentException("Native is nullptr");
            SetHandle(native);
        }

        public override bool IsInvalid => handle == IntPtr.Zero;

        protected override bool ReleaseHandle()
        {
            if(handle != IntPtr.Zero)
            {
                MediaStreamTrackInterop.Destroy(handle);
            }
            return true;
        }
    }

    static class MediaStreamTrackInterop
    {
        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "MediaStreamTrackDestroy")]
        public static extern void Destroy(IntPtr mediaStreamTrackPtr);

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "MediaStreamTrackIsAudioTrack")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool IsAudioTrack(IntPtr mediaStreamTrackPtr);

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "MediaStreamTrackIsAudioTrack")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool IsAudioTrack(MediaStreamTrackSafeHandle mediaStreamTrackPtr);

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "MediaStreamTrackId")]
        public static extern IntPtr Id(MediaStreamTrackSafeHandle hande);
    }

    public sealed class AudioTrack : MediaStreamTrack
    {
        public AudioTrack(IntPtr native) : base(native)
        {
        }
    }

    public sealed class VideoTrack : MediaStreamTrack
    {
        public VideoTrack(IntPtr native) : base(native)
        {
        }
    }

    public abstract class MediaStreamTrack : IDisposable
    {
        readonly MediaStreamTrackSafeHandle _native;

        public MediaStreamTrack(IntPtr native)
        {
            if(native == IntPtr.Zero)
            {
                throw new ArgumentException(nameof(native));
            }
            _native = new MediaStreamTrackSafeHandle(native);
        }

        public bool IsAudioTrack => MediaStreamTrackInterop.IsAudioTrack(_native);

        public void Dispose()
        {
            _native.Dispose();
        }
    }
}