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
        public Task<RTCSessionDescription> CreateAnswerAsync()
        {
            var taskCompletionSource = new TaskCompletionSource<RTCSessionDescription>();
            var callback = new PeerConnectionInterop.CreateAnswerResultCallback((userData, result) =>
            {
                // Free the callback first
                GCHandle.FromIntPtr(userData).Free();

                // Then pass the correct result to the awaiter
                if(result.Success)
                    taskCompletionSource.SetResult(new RTCSessionDescription { Sdp = result.Sdp, Type = result.SdpType });
                else
                    taskCompletionSource.SetException(new CreateAnswerFailedException(result.ErrorMessage ?? string.Empty));
            });
            PeerConnectionInterop.CreateAnswer(
                _handle, callback,
                GCHandleHelper.ToIntPtr(GCHandle.Alloc(callback, GCHandleType.Normal)));
            return taskCompletionSource.Task;
        }

        /// <summary>
        /// Call this when receive session description from remote peers
        /// </summary>
        /// <param name="type"></param>
        /// <param name="sdp"></param>
        /// <exception cref="Errors.SetSessionDescriptionFailedException"></exception>
        /// <returns></returns>
        public Task SetRemoteSessionDescriptionAsync(string type, string sdp)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();
            PeerConnectionInterop.SetSessionDescriptionCallback callback = CreateSdpCallback(taskCompletionSource);
            PeerConnectionInterop.SetRemoteSessionDescription(
                _handle, type, sdp, callback, GCHandleHelper.ToIntPtr(callback));
            return taskCompletionSource.Task;
        }

        /// <summary>
        /// Call this immediately after CreateAnswer()
        /// </summary>
        /// <exception cref="Errors.SetSessionDescriptionFailedException"></exception>
        public Task SetLocalSessionDescriptionAsync(string type, string sdp)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();
            PeerConnectionInterop.SetSessionDescriptionCallback callback = CreateSdpCallback(taskCompletionSource);
            PeerConnectionInterop.SetLocalSessionDescription(
                _handle, type, sdp, callback, GCHandleHelper.ToIntPtr(callback));
            return taskCompletionSource.Task;
        }

        static PeerConnectionInterop.SetSessionDescriptionCallback CreateSdpCallback(TaskCompletionSource<bool> taskCompletionSource)
        {
            return new PeerConnectionInterop.SetSessionDescriptionCallback(
                (userData, sucess, errorMessage) =>
                {
                    GCHandle.FromIntPtr(userData).Free();
                    if(sucess)
                        taskCompletionSource.SetResult(true);
                    else
                        taskCompletionSource.SetException(
                            new SetSessionDescriptionFailedException(errorMessage ?? string.Empty));
                });
        }

        /// <summary>
        /// Only call this after SetRemoteSessionDescriptionAsync completes
        /// </summary>
        /// <exception cref="Errors.AddIceCandidateFailedException"
        public void AddIceCandidate(RTCIceCandidate iceCandidate)
        {
            Require.NotEmpty(iceCandidate);
            if(false == PeerConnectionInterop.AddIceCandidate(
                _handle,
                iceCandidate.SdpMid,
                iceCandidate.SdpMLineIndex,
                iceCandidate.Candidate))
            {
                throw new Errors.AddIceCandidateFailedException("Add ICE candidate failed, check RTC logs");
            }
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
