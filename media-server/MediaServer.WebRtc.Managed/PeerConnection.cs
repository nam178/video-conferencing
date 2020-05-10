using MediaServer.Common.Threading;
using MediaServer.Common.Utils;
using MediaServer.WebRtc.Managed.Errors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace MediaServer.WebRtc.Managed
{
    /// <summary>
    /// </summary>
    /// <remarks>This class is thread safe, due to some method is called from singalling threads, while other from device threads</remarks>
    public class PeerConnection : IDisposable
    {
        readonly PeerConnectionSafeHandle _handle;
        readonly List<(RtpSender RtpSender, MediaStreamTrack Track)> _localTracks = new List<(RtpSender RtpSender, MediaStreamTrack Track)>();
        readonly List<IPendingTask> _pendingTasks = new List<IPendingTask>();

        public PeerConnectionObserver Observer { get; }

        public Guid Id = Guid.NewGuid();

        PeerConnectionInterop.CreateSdpResultCallback _createOfferCallback;
        PeerConnectionInterop.CreateSdpResultCallback _createAnswerCallback;
        PeerConnectionInterop.SetSessionDescriptionCallback _setRemoteSessionDescCallback;
        PeerConnectionInterop.SetSessionDescriptionCallback _setLocalSessionDescCallback;
        object _mutex = new object();
        bool _isClosed;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unmanagedPointer"></param>
        /// <param name="observer">Keep ref only, does not own observer</param>
        internal PeerConnection(IntPtr unmanagedPointer, PeerConnectionObserver observer)
        {
            _handle = new PeerConnectionSafeHandle(unmanagedPointer);
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
            var pendingTask = new PendingTask<RTCSessionDescription>();
            lock(_mutex)
            {
                RequireCallbackNotSet(_createOfferCallback);
                _createAnswerCallback = new PeerConnectionInterop.CreateSdpResultCallback((userData, result) =>
                {
                    _createAnswerCallback = null;
                    CompletePendingTask(pendingTask, result);
                });
                _pendingTasks.Add(pendingTask);
            }
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
            var pendingTask = new PendingTask<RTCSessionDescription>();
            lock(_mutex)
            {
                RequireCallbackNotSet(_createOfferCallback);
                _createOfferCallback = new PeerConnectionInterop.CreateSdpResultCallback((userData, result) =>
                {
                    _createOfferCallback = null;
                    CompletePendingTask(pendingTask, result);
                });
                _pendingTasks.Add(pendingTask);
            }
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
            var pendingTask = new PendingTask<bool>();
            lock(_mutex)
            {
                RequireCallbackNotSet(_setRemoteSessionDescCallback);
                _setRemoteSessionDescCallback = new PeerConnectionInterop.SetSessionDescriptionCallback(
                    (userData, sucess, errorMessage) =>
                    {
                        _setRemoteSessionDescCallback = null;
                        CompletePendingTask(pendingTask, sucess, errorMessage);
                    });
                _pendingTasks.Add(pendingTask);
            }
            PeerConnectionInterop.SetRemoteSessionDescription(_handle, type, sdp, _setRemoteSessionDescCallback, IntPtr.Zero);
            return pendingTask.Task;
        }

        /// <summary>
        /// Call this immediately after CreateAnswer()
        /// </summary>
        /// <exception cref="Errors.SetSessionDescriptionFailedException"></exception>
        public Task SetLocalSessionDescriptionAsync(string type, string sdp)
        {
            var pendingTask = new PendingTask<bool>();
            lock(_mutex)
            {
                RequireCallbackNotSet(_setLocalSessionDescCallback);
                _setLocalSessionDescCallback = new PeerConnectionInterop.SetSessionDescriptionCallback(
                    (userData, sucess, errorMessage) =>
                    {
                        _setLocalSessionDescCallback = null;
                        CompletePendingTask(pendingTask, sucess, errorMessage);
                    });
                _pendingTasks.Add(pendingTask);
            }
            PeerConnectionInterop.SetLocalSessionDescription(_handle, type, sdp, _setLocalSessionDescCallback, IntPtr.Zero);
            return pendingTask.Task;
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
        /// Add the specified track into this PeerConnection's specified stream
        /// </summary>
        /// <param name="track"></param>
        /// <param name="streamId">Id of the stream, doesn't have to exist</param>
        /// <exception cref="AddTrackFailedException"></exception>
        /// <remarks>Can be called from anythread, the libWebRTC will proxy to the correct thread</remarks>
        /// <returns>RtpSender, this PeerConnection takes ownership</returns>
        public RtpSender AddTrack(MediaStreamTrack track, Guid streamId)
        {
            Require.NotNull(track);
            Require.NotEmpty(streamId);
            var rtpSenderPtr = PeerConnectionInterop.AddTrack(_handle, track.Handle, streamId.ToString());
            if(rtpSenderPtr == IntPtr.Zero)
            {
                throw new AddTrackFailedException();
            }
            var rtpSender = new RtpSender(rtpSenderPtr);
            lock(_mutex)
            {
                _localTracks.Add((rtpSender, track));
            }
            return rtpSender;
        }

        /// <summary>
        /// Remove the specified track; The RtpSender and its associated media stream will be disposed.
        /// </summary>
        /// <param name="rtpSender">The track, represented by its RtpSender</param>
        /// <remarks>Can be called from any thread, will be proxied to signalling thread by the lib.</remarks>
        public void RemoveTrack(RtpSender rtpSender)
        {
            lock(_mutex)
            {
                var t = _localTracks.Where(tmp => tmp.RtpSender == rtpSender).FirstOrDefault();
                if(t.RtpSender == null)
                {
                    throw new ArgumentException($"Provided RtpSender not found");
                }
                _localTracks.Remove(t);
                using(t.RtpSender)
                using(t.Track)
                {
                    PeerConnectionInterop.RemoveTrack(_handle, t.RtpSender.Handle);
                }
            }
        }

        /// <summary>
        /// Get local tracks that were added to this PeerConnection.
        /// </summary>
        /// <returns>A copy of RTPSenders for local tracks</returns>
        public RtpSender[] GetLocalTracks()
        {
            lock(_mutex)
            {
                return _localTracks.Select(t => t.RtpSender).ToArray();
            }
        }

        /// <summary>
        /// Close the native PeerConnection, 
        /// note that pending async operations such as the CreateAnswer() 
        /// method may still continue execute after the PeerConnection is closed.
        /// </summary>
        public void Close()
        {
            List<IPendingTask> pendingTasksToPrune;
            lock(_mutex)
            {
                if(_isClosed)
                    throw new InvalidOperationException("Already closed");
                if(_localTracks.Count > 0)
                    throw new InvalidOperationException(
                        "All local tracks must be removed before closing PeerConnection");
                _isClosed = true;
                pendingTasksToPrune = _pendingTasks.ToList();
                _pendingTasks.Clear();
            }

            pendingTasksToPrune.ForEach(l => l.Cancel());

            PeerConnectionInterop.Close(_handle);
        }

        readonly Dictionary<IntPtr, RtpTransceiver> _knownTransceivers = new Dictionary<IntPtr, RtpTransceiver>();
        readonly object _getTransceiversMutex = new object();

        /// <summary>
        /// Get a snapshot of all transceivers associated with this PeerConnection
        /// </summary>
        /// <remarks>This method is thread safe</remarks>
        /// <returns></returns>
        public IReadOnlyList<RtpTransceiver> GetTransceivers()
        {
            IntPtr nativeTransceiverArray;
            int nativeTransceiverArraySize;
            lock(_getTransceiversMutex)
            {
                PeerConnectionInterop.GetTransceivers(_handle, out nativeTransceiverArray, out nativeTransceiverArraySize);
            }

            // When there is no transceivers,
            // The unmanaged code returns size = 0 and nativeTransceiverArray will be IntPtr.Zero,
            // therefore we ignore that.
            if(nativeTransceiverArraySize > 0)
            {
                try
                {
                    for(var i = 0; i < nativeTransceiverArraySize; i++)
                    {
                        var nativeTransceiverIntPtr = Marshal.ReadIntPtr(nativeTransceiverArray, nativeTransceiverArraySize * IntPtr.Size);
                        lock(_getTransceiversMutex)
                        {
                            if(false == _knownTransceivers.ContainsKey(nativeTransceiverIntPtr))
                                _knownTransceivers[nativeTransceiverIntPtr] = new RtpTransceiver(nativeTransceiverIntPtr);
                        }
                    }
                }
                finally
                {
                    PeerConnectionInterop.FreeGetTransceiversResult(_handle, nativeTransceiverArray);
                }
            }
            // Return transceivers as a copy
            lock(_getTransceiversMutex)
            {
                return _knownTransceivers.Values.ToList();
            }
        }

        public void Dispose()
        {
            lock(_mutex)
            {
                Debug.Assert(_localTracks.Count == 0); // All local tracks must be removed prior to disposing this PeerConection
            }
            _handle.Dispose();
        }

        public override string ToString() => $"[PeerConnection Id={Id.ToString().Substring(0, 8)}]";

        void RequireCallbackNotSet(object t)
        {
            if(_isClosed)
                throw new InvalidOperationException("Already closed");
            if(t != null)
                throw new InvalidOperationException("Already setting remote sdp");
        }

        void CompletePendingTask(PendingTask<RTCSessionDescription> pendingTask, PeerConnectionInterop.CreateAnswerResult result)
        {
            lock(_mutex)
            {
                if(_isClosed)
                    return;
                _pendingTasks.Remove(pendingTask);
            }
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
            lock(_mutex)
            {
                if(_isClosed)
                    return;
                _pendingTasks.Remove(pendingTask);
            }
            Task.Run(delegate
            {
                if(sucess)
                    pendingTask.Source.TrySetResult(true);
                else
                    pendingTask.Source.TrySetException(
                        new SetSessionDescriptionFailedException(errorMessage ?? string.Empty));
            });
        }
    }
}
