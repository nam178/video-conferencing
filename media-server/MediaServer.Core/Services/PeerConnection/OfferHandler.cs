﻿using MediaServer.Common.Threading;
using MediaServer.Common.Utils;
using MediaServer.Core.Models;
using MediaServer.Models;
using MediaServer.WebRtc.Managed;
using NLog;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.PeerConnection
{
    sealed class OfferHandler : IOfferHandler
    {
        readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public async Task HandleAsync(IRemoteDevice remoteDevice, Guid? peerConnectionId, RTCSessionDescription request)
        {
            Require.NotNull(request.Sdp);
            Require.NotNull(request.Type);

            // Get user and current IPeerConnection for this device
            var deviceData = remoteDevice.GetCustomData();
            if(null == deviceData.User)
            {
                throw new UnauthorizedAccessException();
            }

            // Try to find the existing PeerConnection to update it
            IPeerConnection peerConnection = null;
            if(peerConnectionId != null)
            {
                peerConnection = deviceData.PeerConnections.FirstOrDefault(p => p.Id == peerConnectionId);
            }

            // Create new PeerConnection
            if(null == peerConnection)
            {

                peerConnection = await CreatePeerConnection(remoteDevice, deviceData.User, request);
                // Save
                deviceData.PeerConnections.Add(peerConnection);
                remoteDevice.SetCustomData(deviceData);
            }
            // Update existing PeerConnection
            else
            {
                await peerConnection.SetRemoteSessionDescriptionAsync(request);
                _logger.Info($"Remote {request} SDP updated for {peerConnection}");
            }

            // Finally do the SDP exchange, order important
            // Create answer and send it.
            var answer = await peerConnection.CreateAnswerAsync();
            _logger.Info($"Answer {answer} created for {peerConnection}");
            remoteDevice.EnqueueSessionDescription(peerConnection.Id, answer);
            _logger.Info($"Answer sent for {peerConnection}");

            // SetLocalSessionDescriptionAsync() must be after SendSessionDescriptionAsync()
            // because it SetLocalSessionDescriptionAsync() generates ICE candidates,
            // and we want to send ICE candidates after remote SDP is set.
            await peerConnection.SetLocalSessionDescriptionAsync(answer);
            _logger.Info($"Local description {answer} set for {peerConnection}");
        }

        async Task<IPeerConnection> CreatePeerConnection(IRemoteDevice remoteDevice, User user, RTCSessionDescription remoteSdp)
        {
            var peerConnection = user.Room.CreatePeerConnection(remoteDevice);

            await peerConnection.SetRemoteSessionDescriptionAsync(remoteSdp);
            _logger.Info($"Remote {remoteSdp} SDP set for {peerConnection}");

            await peerConnection.StartMediaRoutingAsync();
            _logger.Info($"PeerConnection created, user {user}, device {remoteDevice}");

            // This is the first time is PeerConnection is created,
            // we'll add ICE candidate observer
            var _pendingRenegotationRequests = 0;
            peerConnection
                .ObserveIceCandidate((peer, cand) => SendAndForget(remoteDevice, peer, cand))
                .ObserveRenegotiationNeeded(peer =>
                {
                    Interlocked.Increment(ref _pendingRenegotationRequests);

                    // Begin the actual re-negotation
                    // Must do this in a queue to avoid race between requests
                    _logger.Trace($"Re-negotiating with {peerConnection}..");
                    user.Room.RenegotiationQueue.ExecuteAsync(async delegate
                    {
                        try
                        {
                            // Newer request coming in, dropping this
                            if(Interlocked.Decrement(ref _pendingRenegotationRequests) > 0)
                            {
                                _logger.Warn($"Multiple re-negotation triggerd for {peerConnection}, ignoring this one");
                                return;
                            }

                            // Order important here: generate offer first.
                            var offer = await peer.CreateOfferAsync();

                            // Then send the SDP before SetLocalSessionDescription, 
                            // so the sdp processed by remote peer before they process ICE candidates,
                            // those generated from SetLocalSessionDescriptionAsync();
                            remoteDevice.EnqueueSessionDescription(peerConnection.Id, offer);

                            // then send candidates later so they processed after the SDP is processed.
                            await peerConnection.SetLocalSessionDescriptionAsync(offer);
                            _logger.Info($"Re-negotiating offer sent to {peerConnection}.");
                        }
                        catch(Exception ex) when(ex is ObjectDisposedException || ex is TaskCanceledException || ex is InvalidOperationException)
                        {
                            _logger.Warn($"Failed re-negotiating due to PeerConnection closed, will terminate remote device. Err={ex.Message}");
                        }
                        catch(IOException ex)
                        {
                            _logger.Warn($"Failed re-negotiating due to IO, will terminate remote device. Err={ex.Message}");
                        }
                        catch(Exception ex)
                        {
                            _logger.Error(ex, "Unexpected error while re-negotiating");
                        }
                    });
                });

            return peerConnection;
        }

        static void SendAndForget(IRemoteDevice remoteDevice, IPeerConnection peer, RTCIceCandidate cand)
        {
            remoteDevice.EnqueueIceCandidate(peer.Id, cand);
        }
    }
}
