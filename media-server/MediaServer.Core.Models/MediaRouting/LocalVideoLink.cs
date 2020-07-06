﻿using MediaServer.WebRtc.Managed;
using NLog;
using System;
using System.Threading;

namespace MediaServer.Core.Models.MediaRouting
{
    sealed class LocalVideoLink : IDisposable
    {
        readonly VideoTrack _track;
        readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        readonly VideoRouter _parent;

        public PeerConnection TargetPeerConnection { get; }

        public VideoSource VideoSource { get; }

        public RtpTransceiver Transceiver { get; }

        public LocalVideoLink(VideoRouter parent, VideoSource source, PeerConnection target)
        {
            if(source is null)
                throw new ArgumentNullException(nameof(source));
            if(target is null)
                throw new ArgumentNullException(nameof(target));
            if(null == source.VideoTrackSource)
                throw new InvalidProgramException("VideoTrackSource is NULL");

            TargetPeerConnection = target;
            VideoSource = source;

            _parent = parent
                ?? throw new ArgumentNullException(nameof(parent));

            // Create track
            var trackId = Guid.NewGuid();
            _track = parent.PeerConnectionFactory.CreateVideoTrack(trackId.ToString(), source.VideoTrackSource);

            // Find the first available transceiver (or create it)
            GetOrCreateTransceiver(out var transceiver, out var isReusingTransceiver);
            Transceiver = transceiver;

            // Next, set/replace the track:
            Transceiver.ToBusyState(_track);

            // If we're re-using an existing transceivers.
            // Transceiver metadata will need to be sent for clients to update their UIs.
            // If we are creating new transceivers, no need to do this,
            // since PeerConnection will re-negotiate automatically
            if(isReusingTransceiver)
            {
                RaiseTransceiverMetadataUdatedEvent();
            }

            // If stream id has not been set, set it.
            // WebRTC does not allow us to change the stream id, but we don't care either,
            // we just want it to be unique.
            if(string.IsNullOrWhiteSpace(Transceiver.Sender.StreamId))
            {
                Transceiver.Sender.StreamId = Guid.NewGuid().ToString();
            }

            // Add track to peer
            _logger.Debug($"Local track created {_track}");
        }

        void GetOrCreateTransceiver(out RtpTransceiver transceiver, out bool isReusingTransceivers)
        {
            var mediaKind = _track.Kind;
            var transceivers = TargetPeerConnection.GetTransceivers();
            transceiver = null;
            isReusingTransceivers = false;
            for(var i = 0; i < transceivers.Count; i++)
            {
                if(transceivers[i].ReusabilityState == TransceiverReusabilityState.Available
                    && transceivers[i].MediaKind == mediaKind
                    // Notes:
                    // We keep the first 2 transceivers just to receive data, those marked with RecvOnly
                    // and the rest to send data.
                    // (cleaner that way) so ignore them in this search.
                    && transceivers[i].Direction != RtpTransceiverDirection.RecvOnly)
                {
                    transceiver = transceivers[i];
                    break;
                }
            }
            // If no available transceiver, create a new one
            if(transceiver is null)
            {
                transceiver = TargetPeerConnection.AddTransceiver(mediaKind, RtpTransceiverDirection.SendOnly);
                isReusingTransceivers = true;
            }

            transceiver.CustomData = this;
        }

        int _disposed;
        public void Dispose()
        {
            if(Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
            {
                Transceiver.ToFrozenState();
                RaiseTransceiverMetadataUdatedEvent();
                Transceiver.CustomData = null;

                // Notes - Dispose only the track,
                // Don't dispose RtpSender, the PeerConnection owns it.
                _track.Dispose();
            }
        }

        void RaiseTransceiverMetadataUdatedEvent() => _parent.Raise(new TransceiverMetadataUpdatedEvent(
                                TargetPeerConnection.Id,
                                new Common.Media.TransceiverMetadata(
                                    Transceiver.Mid,
                                    VideoSource.Quality,
                                    Transceiver.MediaKind,
                                    VideoSource.VideoClient.Id)
                                ));

        public override string ToString() => $"[LocalVideoLink Source={VideoSource}, Target={TargetPeerConnection}, Track={_track}]";
    }
}