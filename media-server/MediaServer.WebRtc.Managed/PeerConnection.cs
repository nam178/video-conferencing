using MediaServer.Common.Media;
using MediaServer.Common.Patterns;
using MediaServer.Common.Utils;
using MediaServer.WebRtc.Managed.Errors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace MediaServer.WebRtc.Managed
{
    /// <summary>
    /// </summary>
    /// <remarks>Call methods must be called from signalling thread. Not thread safe.</remarks>
    public class PeerConnection : IDisposable
    {
        readonly PeerConnectionSafeHandle _handle;
        readonly List<Common.Patterns.IObserver> _pendingObservers = new List<Common.Patterns.IObserver>();
        readonly Dictionary<IntPtr, RtpTransceiver> _knownTransceivers = new Dictionary<IntPtr, RtpTransceiver>();
        readonly RtcThread _signallingThread;

        public PeerConnectionObserver Observer { get; }

        public Guid Id { get; }

        PeerConnectionInterop.CreateSdpResultCallback _createOfferCallback;
        PeerConnectionInterop.CreateSdpResultCallback _createAnswerCallback;
        PeerConnectionInterop.SetSessionDescriptionCallback _setRemoteSessionDescCallback;
        PeerConnectionInterop.SetSessionDescriptionCallback _setLocalSessionDescCallback;
        bool _isClosed;

        // Helps debugging easier by hard-coding the first few PeerConnection ids
        static IReadOnlyList<Guid> _debugIds = new Guid[]
        {
            Guid.Parse("c86a51c1-188d-4521-8c0c-8aff999fc645"),
            Guid.Parse("1d6b12a4-e039-492a-a9a3-a11c4825fa8e"),
            Guid.Parse("267ea6d1-3bdb-4e1e-9b78-5324e3a02539")
        };
        static int _debugIdPtr = -1;

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
            Id = Guid.NewGuid();

            if(Debugger.IsAttached)
            {
                var index = Interlocked.Increment(ref _debugIdPtr);
                if(index < _debugIds.Count)
                {
                    Id = _debugIds[index];
                }
            }
        }

        /// <summary>
        /// Generate an answer after the remote sdp is set
        /// </summary>
        /// <returns></returns>
        /// <remarks>Can be called from any thread</remarks>
        /// <exception cref="Errors.CreateAnswerFailedException" />
        public void CreateAnswer(Observer<RTCSessionDescription> observer)
        {
            if(observer is null)
                throw new ArgumentNullException(nameof(observer));
            SafetyCheck();
            RequireCallbackNotSet(_createAnswerCallback);
            _createAnswerCallback = new PeerConnectionInterop.CreateSdpResultCallback((userData, result) =>
            {
                _signallingThread.EnsureCurrentThread();
                _createAnswerCallback = null;
                Complete(observer, result);
            });
            _pendingObservers.Add(observer);
            PeerConnectionInterop.CreateAnswer(_handle, _createAnswerCallback, IntPtr.Zero);
        }

        /// <summary>
        /// Generate an SDP offer.
        /// </summary>
        /// <remarks>Can be called from any thread</remarks>
        /// <returns></returns>
        public void CreateOffer(Observer<RTCSessionDescription> observer)
        {
            SafetyCheck();
            RequireCallbackNotSet(_createOfferCallback);
            _createOfferCallback = new PeerConnectionInterop.CreateSdpResultCallback((userData, result) =>
            {
                _signallingThread.EnsureCurrentThread();
                _createOfferCallback = null;
                Complete(observer, result);
            });
            _pendingObservers.Add(observer);
            PeerConnectionInterop.CreateOffer(_handle, _createOfferCallback, IntPtr.Zero);
        }

        /// <summary>
        /// Call this when receive session description from remote peers
        /// </summary>
        /// <param name="type"></param>
        /// <param name="sdp"></param>
        /// <exception cref="Errors.SetSessionDescriptionFailedException"></exception>
        /// <returns></returns>
        public void SetRemoteSessionDescription(string type, string sdp, Observer observer)
        {
            if(type is null)
                throw new ArgumentNullException(nameof(type));
            if(sdp is null)
                throw new ArgumentNullException(nameof(sdp));
            if(observer is null)
                throw new ArgumentNullException(nameof(observer));

            SafetyCheck();
            RequireCallbackNotSet(_setRemoteSessionDescCallback);
            _setRemoteSessionDescCallback = new PeerConnectionInterop.SetSessionDescriptionCallback(
                (userData, sucess, errorMessage) =>
                {
                    _signallingThread.EnsureCurrentThread();
                    _setRemoteSessionDescCallback = null;
                    Complete(observer, sucess, errorMessage);
                });
            _pendingObservers.Add(observer);
            PeerConnectionInterop.SetRemoteSessionDescription(_handle, type, sdp, _setRemoteSessionDescCallback, IntPtr.Zero);
        }

        /// <summary>
        /// Call this immediately after CreateAnswer()
        /// </summary>
        /// <exception cref="Errors.SetSessionDescriptionFailedException"></exception>
        public void SetLocalSessionDescription(string type, string sdp, Observer observer)
        {
            if(type is null)
                throw new ArgumentNullException(nameof(type));
            if(sdp is null)
                throw new ArgumentNullException(nameof(sdp));
            if(observer is null)
                throw new ArgumentNullException(nameof(observer));

            SafetyCheck();
            RequireCallbackNotSet(_setLocalSessionDescCallback);
            _setLocalSessionDescCallback = new PeerConnectionInterop.SetSessionDescriptionCallback(
                (userData, sucess, errorMessage) =>
                {
                    _signallingThread.EnsureCurrentThread();
                    _setLocalSessionDescCallback = null;
                    Complete(observer, sucess, errorMessage);
                });
            _pendingObservers.Add(observer);
            PeerConnectionInterop.SetLocalSessionDescription(_handle, type, sdp, _setLocalSessionDescCallback, IntPtr.Zero);
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

            _pendingObservers.ToList().ForEach(l => l.Error("Cancelled"));
            _pendingObservers.Clear();

            PeerConnectionInterop.Close(_handle);
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

        public RtpTransceiver AddTransceiver(MediaKind kind, RtpTransceiverDirection direction)
        {
            SafetyCheck();
            if(kind == MediaKind.Data)
                throw new NotSupportedException();

            var transceiverPtr = PeerConnectionInterop.AddTransceiver(_handle, kind == MediaKind.Audio, direction);
            if(transceiverPtr == IntPtr.Zero)
                throw new AddTransceiverFailedException();

            var tmp = new RtpTransceiver(transceiverPtr, _signallingThread);
            _knownTransceivers.Add(transceiverPtr, tmp);
            return tmp;
        }

        /// <summary>
        /// No longer used but still keep here for integration tests
        /// </summary>
        /// <param name="track"></param>
        /// <param name="streamId">Id of the stream, doesn't have to exist</param>
        /// <exception cref="AddTrackFailedException"></exception>
        /// <remarks>Can be called from anythread, the libWebRTC will proxy to the correct thread</remarks>
        /// <returns>RtpSender, this PeerConnection takes ownership</returns>
        internal RtpSender AddTrack(MediaStreamTrack track, Guid streamId)
        {
            SafetyCheck();
            Require.NotNull(track);
            Require.NotEmpty(streamId);
            var rtpSenderPtr = PeerConnectionInterop.AddTrack(_handle, track.Handle, streamId.ToString());
            if(rtpSenderPtr == IntPtr.Zero)
            {
                throw new AddTrackFailedException();
            }
            var rtpSender = new RtpSender(rtpSenderPtr, _signallingThread);
            return rtpSender;
        }

        /// <summary>
        /// No longer used but still keep it here for tests
        /// </summary>
        /// <param name="rtpSender">The track, represented by its RtpSender</param>
        /// <remarks>Can be called from any thread, will be proxied to signalling thread by the lib.</remarks>
        internal void RemoveTrack(RtpSender rtpSender)
        {
            SafetyCheck();
           PeerConnectionInterop.RemoveTrack(_handle, rtpSender.Handle);
        }

        void RequireCallbackNotSet(object t)
        {
            if(_isClosed)
                throw new InvalidOperationException("Already closed");
            if(t != null)
                throw new InvalidOperationException("Already setting remote sdp");
        }

        void SafetyCheck()
        {
            if(Interlocked.CompareExchange(ref _disposed, 0, 0) == 1)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            _signallingThread.EnsureCurrentThread();
        }

        void Complete(Observer observer, bool sucess, string errorMessage)
        {
            try
            {
                _pendingObservers.Remove(observer);
                if(sucess)
                    observer.Success();
                else
                    observer.Error(errorMessage ?? string.Empty);
            }
            // Notes
            // Observer may already completed by the Close() method,
            // we ignore the error in such case.
            catch(ObserverAlreadyCompletedException) { }
        }

        void Complete(
            Observer<RTCSessionDescription> observer,
            PeerConnectionInterop.CreateAnswerResult result)
        {
            try
            {
                _pendingObservers.Remove(observer);
                if(result.Success)
                    observer.Result(new RTCSessionDescription { Sdp = result.Sdp, Type = result.SdpType });
                else
                    observer.Error(result.ErrorMessage ?? string.Empty);
            }
            // Notes
            // Observer may already completed by the Close() method,
            // we ignore the error in such case.
            catch(ObserverAlreadyCompletedException) { }
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
