﻿using MediaServer.Common.Media;
using MediaServer.Common.Patterns;
using MediaServer.Common.Threading;
using MediaServer.Common.Utils;
using MediaServer.WebRtc.Managed;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaServer.Core.Models.MediaRouting
{
    sealed class VideoRouter : IVideoRouter
    {
        readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        readonly IThread _signallingThread;
        readonly ClientCollection _clients = new ClientCollection();
        readonly LocalVideoLinkCollection _localVideoLinks = new LocalVideoLinkCollection();
        readonly RemoteVideoLinkCollection _remoteVideoLinks = new RemoteVideoLinkCollection();

        IObserver<TransceiverMetadataUpdatedEvent> _transceiverMetadataObserver;

        internal PeerConnectionFactory PeerConnectionFactory { get; }

        public VideoRouter(
            IThread signallingThread,
            PeerConnectionFactory peerConnectionFactory)
        {
            _signallingThread = signallingThread
                ?? throw new ArgumentNullException(nameof(signallingThread));
            PeerConnectionFactory = peerConnectionFactory
                ?? throw new ArgumentNullException(nameof(peerConnectionFactory));
        }

        public void AddRemoteDevice(IRemoteDevice remoteDevice)
        {
            Require.NotNull(remoteDevice);
            _signallingThread.EnsureCurrentThread();

            var client = _clients.Add(remoteDevice);
            _logger.Info($"Added {client} into {_clients}");
        }

        public void SetRemoteTransceiverMetadata(TransceiverMetadata transceiverMetadata)
        {
            // Ignore audio tracks for now
            if(transceiverMetadata.Kind != MediaKind.Video)
                return;
            _signallingThread.EnsureCurrentThread();

            // Prepare the source for this quality, if it was not created
            var client = _clients.GetOrThrow(transceiverMetadata.SourceDeviceId);
            var videoSource = client.VideoSources.ContainsKey(transceiverMetadata.Quality)
                ? client.VideoSources[transceiverMetadata.Quality]
                : null;
            if(null == videoSource)
            {
                videoSource = _clients.CreateVideoSource(transceiverMetadata.SourceDeviceId, transceiverMetadata.Quality);
                videoSource.VideoTrackSource = new PassiveVideoTrackSource();
                videoSource.VideoSinkAdapter = new VideoSinkAdapter(videoSource.VideoTrackSource, false);
                _logger.Info($"Created {videoSource}");
                OnVideoSourceAdded(client, videoSource, transceiverMetadata.Quality);
            }

            // Flag this source to say it should be 
            // linked with the provided track
            videoSource.ExpectedTransceiverMid = transceiverMetadata.TransceiverMid;
            _logger.Info($"Associated {videoSource} with transceiver mid {transceiverMetadata.TransceiverMid}");
        }

        public void RemoveRemoteDevice(IRemoteDevice remoteDevice)
        {
            _signallingThread.EnsureCurrentThread();
            // Remove the video sources
            var client = _clients.GetOrThrow(remoteDevice.Id);
            if(client.PeerConnections.Count > 0)
                throw new InvalidProgramException("Client's PeerConnections were not removed");

            // Remove each video source
            foreach(var kv in client.VideoSources)
            {
                var videoSource = kv.Value;

                if(videoSource.VideoTrackSource == null)
                    throw new InvalidProgramException($"{nameof(videoSource.VideoTrackSource)} is null");
                if(videoSource.VideoSinkAdapter == null)
                    throw new InvalidProgramException($"{nameof(videoSource.VideoSinkAdapter)} is null");

                using(videoSource.VideoTrackSource)
                using(videoSource.VideoSinkAdapter)
                {
                    // At this point, there must be no remote link to this VideoSource,
                    // because they are removed when PeerConnection closes.
                    // If not, it's a programmer mistake.
                    if(_remoteVideoLinks.Exists(videoSource))
                    {
                        throw new InvalidProgramException(
                            "All remote VideoSource must be removed at the time client left.");
                    }

                    // Disconnect all any remote video links.
                    _localVideoLinks.RemoveByVideoSource(videoSource);
                    _logger.Info($"Removed {videoSource} from {_localVideoLinks}");
                }
            }

            // Remove the client
            _clients.Remove(remoteDevice);
            _logger.Info($"Removed client {client} from {_clients}");
        }

        public void AckTransceiverMetadata(IPeerConnection peerConnection, string transceiverMid)
        {
            Require.NotNull(peerConnection);
            Require.NotNull(transceiverMid);
            _signallingThread.EnsureCurrentThread();

            var videoClient = _clients.GetOrThrow(peerConnection.Device.Id);
            if(null == videoClient)
                throw new InvalidProgramException($"PeerConnection {peerConnection} does not belong to this router");

            // Get the transceiver, if it is frozen, mark it as available.
            var transceivers = peerConnection.Native.GetTransceivers();
            for(int i = 0; i < transceivers.Count; i++)
            {
                if(transceivers[i].ReusabilityState == TransceiverReusabilityState.Fronzen
                    && transceivers[i].Mid == transceiverMid)
                {
                    transceivers[i].ToAvailableState();
                    _logger.Debug($"Marked {transceivers[i]} as available for re-use");
                    break;
                }
            }
        }

        public IReadOnlyList<TransceiverMetadata> GetLocalTransceiverMetadata(IPeerConnection peerConnection)
        {
            Require.NotNull(peerConnection);
            _signallingThread.EnsureCurrentThread();

            var videoClient = _clients.GetOrThrow(peerConnection.Device.Id);
            if(null == videoClient)
                throw new InvalidProgramException($"PeerConnection {peerConnection} does not belong to this router");
            var transceivers = peerConnection.Native.GetTransceivers();
            if(null == transceivers)
                throw new NullReferenceException(nameof(transceivers));

            // We only generate transceiver metadata those are:
            //  1. Currently being used (TransceiverReusabilityState busy)
            //  2. Has been negotiated (Mid is not null)
            var result = new List<TransceiverMetadata>();
            for(var i = 0; i < transceivers.Count; i++)
            {
                if(transceivers[i].ReusabilityState == TransceiverReusabilityState.Busy
                    && false == string.IsNullOrWhiteSpace(transceivers[i].Mid))
                {
                    if(null == transceivers[i].CustomData)
                        throw new InvalidProgramException("Transceiver custom data is  NULL");
                    var localVideoLink = transceivers[i].CustomData as LocalVideoLink;
                    if(null == transceivers[i].CustomData)
                        throw new InvalidProgramException($"Transceiver custom data is not {nameof(LocalVideoLink)}");

                    result.Add(new TransceiverMetadata(
                        transceivers[i].Mid,
                        videoClient.DesiredMediaQuality,
                        transceivers[i].MediaKind,
                        localVideoLink.VideoSource.Client.Device.Id));
                }
            }
            return result;
        }

        public IDisposable Subscribe(IObserver<TransceiverMetadataUpdatedEvent> observer)
        {
            if(_transceiverMetadataObserver != null)
            {
                throw new NotSupportedException();
            }
            _transceiverMetadataObserver = observer;
            return new Disposer(() => _transceiverMetadataObserver = null);
        }

        public void AddPeerConnection(IRemoteDevice remoteDevice, IPeerConnection peerConnection)
        {
            Require.NotNull(peerConnection);
            _signallingThread.EnsureCurrentThread();

            // Add PeerConnection into VideoClient
            var client = _clients.GetOrThrow(remoteDevice.Id);
            if(client.PeerConnections.Contains(peerConnection))
            {
                throw new InvalidProgramException($"{peerConnection} already added into {client}");
            }
            client.PeerConnections.Add(peerConnection);
            _logger.Info($"Added {peerConnection} into {client}, total PeerConnections for this client={client.PeerConnections.Count}");

            // If this PeerConnetion has some existing transceivers,
            // add them.
            // TODO: listen for future transceivers and add them too, too lazy to implement this for now.
            var currenTransceivers = peerConnection.Native.GetTransceivers();
            foreach(var transceiver in currenTransceivers)
            {
                OnRemoteTrackAdded(client, peerConnection, transceiver);
            }

            // Link this PeerConnection with any existing local VideoSources
            foreach(var other in _clients
                .OtherThan(client)
                .Where(other => other.VideoSources.ContainsKey(client.DesiredMediaQuality)))
            {
                var localVideoLink = new LocalVideoLink(
                    this,
                    other.VideoSources[client.DesiredMediaQuality],
                    peerConnection);
                _localVideoLinks.Add(localVideoLink);
                _logger.Debug($"Added {localVideoLink} into {_localVideoLinks}");
            }
        }

        public void RemovePeerConnection(IRemoteDevice remoteDevice, IPeerConnection peerConnection)
        {
            _signallingThread.EnsureCurrentThread();
            // Remove all video links that was created for this PeerConnection
            _localVideoLinks.RemoveByPeerConnection(peerConnection);
            _logger.Info($"Removed all video links for {peerConnection} from {_localVideoLinks}");

            // Remove all video links that was created for this PeerConnetion's remote tracks
            _remoteVideoLinks.RemoveByPeerConnection(peerConnection);
            _logger.Info($"Removed all video links for {peerConnection} from {_remoteVideoLinks}");

            // Remove this PeerConnection from VideoClient
            var client = _clients.GetOrThrow(remoteDevice.Id);
            var removed = client.PeerConnections.Remove(peerConnection);
            if(!removed)
                throw new InvalidProgramException("PeerConnection did not removed from memory");
            _logger.Info($"Removed {peerConnection} from {client}, remaining PeerConnections for this client={client.PeerConnections.Count}");
        }

        internal void Submit(TransceiverMetadataUpdatedEvent e) => _transceiverMetadataObserver.OnNext(e);

        internal void OnRemoteTrackAdded(Client client, IPeerConnection peerConnection, RtpTransceiver transceiver)
        {
            _signallingThread.EnsureCurrentThread();
            Require.NotNull(peerConnection);
            VideoRouterThrowHelper.WhenInvalidReceiver(transceiver);

            // Ignore audio tracks for now
            if(transceiver.Receiver.Track.Kind != MediaKind.Video)
                return;

            // What's the video source that this track should be connected to?
            var transceiverMid = transceiver.Mid;
            var videoSource = client.VideoSources
                .FirstOrDefault(kv => string.Equals(kv.Value.ExpectedTransceiverMid, transceiverMid, StringComparison.InvariantCultureIgnoreCase))
                .Value;
            // videoSource could be null when the client doesn't have any input devices,
            // therefore it never calls SetRemoteTransceiverMetadata()
            if(videoSource != null)
            {
                VideoRouterThrowHelper.WhenSourceIsEmpty(videoSource, transceiver.Receiver);

                var remoteVideoLink = new RemoteVideoLink(peerConnection, videoSource, transceiver.Receiver);
                _remoteVideoLinks.AddOrUpdate(remoteVideoLink);
                _logger.Info($"Added {remoteVideoLink} into {_remoteVideoLinks}");
            }
        }

        internal void OnRemoteTrackRemoved(RtpTransceiver transceiver)
        {
            _signallingThread.EnsureCurrentThread();
            VideoRouterThrowHelper.WhenInvalidReceiver(transceiver);

            _remoteVideoLinks.RemoveByRemoteTrack(transceiver.Receiver);
            _logger.Info($"Removed {transceiver.Receiver} from {_remoteVideoLinks}");
        }

        void OnVideoSourceAdded(Client client, VideoSource videoSource, MediaQuality mediaQuality)
        {
            _signallingThread.EnsureCurrentThread();

            // As new video source is added, 
            // we'll have to connect this source to any existing sending-PeerConnection
            foreach(var otherClients in _clients
                .OtherThan(client)
                .Where(other => other.DesiredMediaQuality == mediaQuality && other.PeerConnections.Count > 0))
            {
                var localVideoLink = new LocalVideoLink(
                    this,
                    videoSource,
                    otherClients.PeerConnections[0]);
                _localVideoLinks.Add(localVideoLink);
                _logger.Debug($"Added {localVideoLink} into {_localVideoLinks}");
            }
        }
    }
}
