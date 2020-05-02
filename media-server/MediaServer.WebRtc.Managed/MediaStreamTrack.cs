using System;
using System.Runtime.InteropServices;

namespace MediaServer.WebRtc.Managed
{
    public abstract class MediaStreamTrack : IDisposable
    {
        public enum Kind
        {
            Audio,
            Video
        }

        internal MediaStreamTrackSafeHandle Handle { get; }

        public Kind TrackKind => MediaStreamTrackInterop.IsAudioTrack(Handle) ? Kind.Audio : Kind.Video;

        public string Id { get; private set; }

        public MediaStreamTrack(IntPtr handle)
        {
            if(handle == IntPtr.Zero)
                throw new ArgumentException(nameof(handle));
            Handle = new MediaStreamTrackSafeHandle(handle);

            var nativeIdString = MediaStreamTrackInterop.Id(Handle);
            if(nativeIdString == IntPtr.Zero)
                Id = null;
            else
                Id = Marshal.PtrToStringAnsi(nativeIdString);
        }

        public void Dispose()
        {
            Handle.Dispose();
        }

        public override string ToString() => $"[{GetType().Name}, Id={Id.Substring(0, 8)}]";
    }
}