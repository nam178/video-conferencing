using MediaServer.Common.Threading;
using MediaServer.Common.Utils;
using MediaServer.WebRtc.Managed.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace MediaServer.WebRtc.Managed
{
    /// <summary>
    /// </summary>
    /// <remarks>This class is thread safe, due to some method is called from singalling threads, while other from device threads</remarks>
    public class PeerConnection : IDisposable
    {
        readonly PeerConnectionSafeHandle _handle;
        readonly List<IPendingTask> _pendingTasks = new List<IPendingTask>();
        readonly Dictionary<IntPtr, RtpTransceiver> _knownTransceivers = new Dictionary<IntPtr, RtpTransceiver>();
        readonly RtcThread _signallingThread;

        public PeerConnectionObserver Observer { get; }

        public Guid Id = Guid.NewGuid();

        PeerConnectionInterop.CreateSdpResultCallback _createOfferCallback;
        PeerConnectionInterop.CreateSdpResultCallback _createAnswerCallback;
        PeerConnectionInterop.SetSessionDescriptionCallback _setRemoteSessionDescCallback;
        PeerConnectionInterop.SetSessionDescriptionCallback _setLocalSessionDescCallback;
        bool _isClosed;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unmanagedPointer"></param>
        /// <param name="observer">Keep ref only, does not own observer</param>
        internal PeerConnection(IntPtr unmanagedPointer, PeerConnectionObserver observer, RtcThread signallingThread)
        {
            _handle = new PeerConnectionSafeHandle(unmanagedPointer);
            _signallingThread = signallingThread
                ?? throw new ArgumentNullException(nameof(signallingThread));
            Observer = observer;
        }

        /// <summary>
        /// Generate an answer after the remote sdp is set
        /// </summary>
        /// <returns></returns>
        /// <remarks>Can be called from any thread</remarks>
        /// <exception cref="Errors.CreateAnswerFailedException" />
        public Task<RTCSessionDescription> CreateAnswerAsync()
        {
            SafetyCheck();
            var pendingTask = new PendingTask<RTCSessionDescription>();
            RequireCallbackNotSet(_createAnswerCallback);
            _createAnswerCallback = new PeerConnectionInterop.CreateSdpResultCallback((userData, result) =>
            {
                _signallingThread.EnsureCurrentThread();
                _createAnswerCallback = null;
                CompletePendingTask(pendingTask, result);
            });
            _pendingTasks.Add(pendingTask);
            PeerConnectionInterop.CreateAnswer(_handle, _createAnswerCallback, IntPtr.Zero);
            return pendingTask.Task;
        }

        /// <summary>
        /// Generate an SDP offer.
        /// </summary>
        /// <remarks>Can be called from any thread</remarks>
        /// <returns></returns>
        public Task<RTCSessionDescription> CreateOfferAsync()
        {
            SafetyCheck();
            var pendingTask = new PendingTask<RTCSessionDescription>();
            RequireCallbackNotSet(_createOfferCallback);
            _createOfferCallback = new PeerConnectionInterop.CreateSdpResultCallback((userData, result) =>
            {
                _signallingThread.EnsureCurrentThread();
                _createOfferCallback = null;
                CompletePendingTask(pendingTask, result);
            });
            _pendingTasks.Add(pendingTask);
            PeerConnectionInterop.CreateOffer(_handle, _createOfferCallback, IntPtr.Zero);
            return pendingTask.Task;
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
            SafetyCheck();
            var pendingTask = new PendingTask<bool>();
            RequireCallbackNotSet(_setRemoteSessionDescCallback);
            _setRemoteSessionDescCallback = new PeerConnectionInterop.SetSessionDescriptionCallback(
                (userData, sucess, errorMessage) =>
                {
                    _signallingThread.EnsureCurrentThread();
                    _setRemoteSessionDescCallback = null;
                    CompletePendingTask(pendingTask, sucess, errorMessage);
                });
            _pendingTasks.Add(pendingTask);
            PeerConnectionInterop.SetRemoteSessionDescription(_handle, type, sdp, _setRemoteSessionDescCallback, IntPtr.Zero);
            return pendingTask.Task;
        }

        /// <summary>
        /// Call this immediately after CreateAnswer()
        /// </summary>
        /// <exception cref="Errors.SetSessionDescriptionFailedException"></exception>
        public Task SetLocalSessionDescriptionAsync(string type, string sdp)
        {
            SafetyCheck();
            var pendingTask = new PendingTask<bool>();
            RequireCallbackNotSet(_setLocalSessionDescCallback);
            _setLocalSessionDescCallback = new PeerConnectionInterop.SetSessionDescriptionCallback(
                (userData, sucess, errorMessage) =>
                {
                    _signallingThread.EnsureCurrentThread();
                    _setLocalSessionDescCallback = null;
                    CompletePendingTask(pendingTask, sucess, errorMessage);
                });
            _pendingTasks.Add(pendingTask);
            PeerConnectionInterop.SetLocalSessionDescription(_handle, type, sdp, _setLocalSessionDescCallback, IntPtr.Zero);
            return pendingTask.Task;
        }

        /// <summary>
        /// Only call this after SetRemoteSessionDescriptionAsync completes
        /// </summary>
        /// <exception cref="Errors.AddIceCandidateFailedException"
        public void AddIceCandidate(RTCIceCandidate iceCandidate)
        {
            SafetyCheck();
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
        public void Close()
        {
            SafetyCheck();
            if(_isClosed)
                throw new InvalidOperationException("Already closed");
            _isClosed = true;

            PeerConnectionInterop.Close(_handle);

            _pendingTasks.ToList().ForEach(l => l.Cancel());
            _pendingTasks.Clear();
        }

        /// <summary>
        /// Get a snapshot of all transceivers associated with this PeerConnection.
        /// This PeerConnection owns the returned transceivers.
        /// </summary>
        /// <remarks>This method is thread safe</remarks>
        /// <returns></returns>
        public IReadOnlyList<RtpTransceiver> GetTransceivers()
        {
            SafetyCheck();

            PeerConnectionInterop.GetTransceivers(_handle,
                out var nativeTransceiverArray,
                out var nativeTransceiverArraySize);

            // When there is no transceivers,
            // The unmanaged code returns size = 0 and nativeTransceiverArray will be IntPtr.Zero,
            // therefore we ignore that.
            if(nativeTransceiverArraySize > 0)
            {
                try
                {
                    for(var i = 0; i < nativeTransceiverArraySize; i++)
                    {
                        var nativeTransceiverIntPtr = Marshal.ReadIntPtr(nativeTransceiverArray, i * IntPtr.Size);
                        if(false == _knownTransceivers.ContainsKey(nativeTransceiverIntPtr))
                        {
                            _knownTransceivers[nativeTransceiverIntPtr] = new RtpTransceiver(
                                nativeTransceiverIntPtr,
                                _signallingThread);
                        }
                    }
                }
                finally
                {
                    PeerConnectionInterop.FreeGetTransceiversResult(_handle, nativeTransceiverArray);
                }
            }
            // Return transceivers as a copy
            return _knownTransceivers.Values.ToList();
        }

        public RtpTransceiver AddTransceiver(MediaKind kind)
        {
            SafetyCheck();
            if(kind == MediaKind.Data)
                throw new NotSupportedException();

            var transceiverPtr = PeerConnectionInterop.AddTransceiver(_handle, kind == MediaKind.Audio);
            if(transceiverPtr == IntPtr.Zero)
                throw new AddTransceiverFailedException();

            var tmp = new RtpTransceiver(transceiverPtr, _signallingThread);
            _knownTransceivers.Add(transceiverPtr, tmp);
            return tmp;
        }

        void RequireCallbackNotSet(object t)
        {
            if(_isClosed)
                throw new InvalidOperationException("Already closed");
            if(t != null)
                throw new InvalidOperationException("Already setting remote sdp");
        }

        void CompletePendingTask(PendingTask<RTCSessionDescription> pendingTask, PeerConnectionInterop.CreateAnswerResult result)
        {
            if(_isClosed)
                return;
            _pendingTasks.Remove(pendingTask);
            Task.Run(delegate
            {
                if(result.Success)
                    pendingTask.Source.TrySetResult(new RTCSessionDescription { Sdp = result.Sdp, Type = result.SdpType });
                else
                    pendingTask.Source.TrySetException(new CreateAnswerFailedException(result.ErrorMessage ?? string.Empty));
            });
        }

        void CompletePendingTask(PendingTask<bool> pendingTask, bool sucess, string errorMessage)
        {
            if(_isClosed)
                return;
            _pendingTasks.Remove(pendingTask);
            Task.Run(delegate
            {
                if(sucess)
                    pendingTask.Source.TrySetResult(true);
                else
                    pendingTask.Source.TrySetException(
                        new SetSessionDescriptionFailedException(errorMessage ?? string.Empty));
            });
        }

        void SafetyCheck()
        {
            _signallingThread.EnsureCurrentThread();
            if(Interlocked.CompareExchange(ref _disposed, 0, 0) == 1)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }


        int _disposed;
        public void Dispose()
        {
            if(Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
            {
                foreach(var transceiver in _knownTransceivers)
                {
                    transceiver.Value.Dispose();
                }
                _knownTransceivers.Clear();
                _handle.Dispose();
            }
        }

        public override string ToString() => $"[PeerConnection Id={Id.ToString().Substring(0, 8)}]";
    }
}
