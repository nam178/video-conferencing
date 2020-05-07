using MediaServer.Common.Utils;
using NLog;
using System;
using System.Runtime.InteropServices;

namespace MediaServer.WebRtc.Managed
{
    public sealed class RtcThread
    {
        readonly IntPtr _handle; // Don't disposed this, as the unmanaged code owns the thread
        readonly ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        public bool IsCurrent => RtcThreadInterops.IsCurrent(_handle);

        public RtcThread(IntPtr intPtr)
        {
            if(intPtr == IntPtr.Zero)
                throw new ArgumentException(nameof(intPtr));
            _handle = intPtr;
        }

        public void Post(Action<object> handler, object userData)
        {
            var actualHandler = new RtcThreadInterops.Handler(userDataPtr =>
            {
                try
                {
                    handler(userData);
                }
                catch(Exception ex)
                {
                    _logger.Error(ex, "Action posted to unmanaged thread caused an exception");
                }
                finally
                {
                    GCHandle.FromIntPtr(userDataPtr).Free();
                }
            });

            RtcThreadInterops.Post(_handle, actualHandler, GCHandleHelper.ToIntPtr(actualHandler));
        }
    }
}