using System;
using System.Runtime.InteropServices;

namespace MediaServer.WebRtc.Managed
{
    public abstract class MediaStreamTrack : IDisposable
    {
        readonly MediaStreamTrackSafeHandle _native;

        public bool IsAudioTrack => MediaStreamTrackInterop.IsAudioTrack(_native);

        public string Id { get; private set; }

        public MediaStreamTrack(IntPtr native)
        {
            if(native == IntPtr.Zero)
            {
                throw new ArgumentException(nameof(native));
            }
            _native = new MediaStreamTrackSafeHandle(native);

            var nativeIdString = MediaStreamTrackInterop.Id(_native);
            if(nativeIdString == IntPtr.Zero)
                Id = null;
            else
                Id = Marshal.PtrToStringAnsi(nativeIdString);
        }

        public void Dispose()
        {
            _native.Dispose();
        }

        public override string ToString() => $"[{GetType().Name}, Id={Id}]";
    }
}