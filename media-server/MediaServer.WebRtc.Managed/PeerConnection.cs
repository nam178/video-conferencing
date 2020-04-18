using MediaServer.Common.Utils;
using MediaServer.WebRtc.Managed.Errors;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace MediaServer.WebRtc.Managed
{
    public class PeerConnection : IDisposable
    {
        readonly PeerConnectionSafeHandle _handle;

        public PeerConnectionObserver Observer { get; }

        internal PeerConnection(IntPtr unmanagedPointer)
        {
            _handle = new PeerConnectionSafeHandle(unmanagedPointer);
        }

        /// <summary>
        /// Generate an answer after the remote sdp is set
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Errors.CreateAnswerFailedException" />
        public Task<RTCSessionDescription> CreateAnswer()
        {
            var taskCompletionSource = new TaskCompletionSource<RTCSessionDescription>();
            var callback = new PeerConnectionInterop.CreateAnswerResultCallback((userData, result) =>
            {
                // Free the callback first
                GCHandle.FromIntPtr(userData).Free();

                // Then pass the correct result to the awaiter
                if(result.Success)
                    taskCompletionSource.SetResult(new RTCSessionDescription { Sdp = result.Sdp, SdpType = result.SdpType });
                else
                    taskCompletionSource.SetException(new CreateAnswerFailedException(result.ErrorMessage ?? string.Empty));
            });
            PeerConnectionInterop.CreateAnswer(
                _handle, callback, 
                GCHandleHelper.ToIntPtr(GCHandle.Alloc(callback, GCHandleType.Normal)));
            return taskCompletionSource.Task;
        }

        public void SetRemoteSessionDescription(string type, string sdp)
        {
            PeerConnectionInterop.SetRemoteSessionDescription(_handle, type, sdp);
        }

        /// <summary>
        /// Close the native PeerConnection, 
        /// note that pending async operations such as the CreateAnswer() 
        /// method may still continue execute after the PeerConnection is closed.
        /// </summary>
        public void Close() => PeerConnectionInterop.Close(_handle);

        public void Dispose() => _handle.Dispose();
    }
}
