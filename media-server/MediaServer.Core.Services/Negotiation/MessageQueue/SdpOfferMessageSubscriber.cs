﻿using MediaServer.Common.Patterns;
using MediaServer.Core.Models;
using MediaServer.WebRtc.Common;
using NLog;
using System;

namespace MediaServer.Core.Services.Negotiation.MessageQueue
{
    sealed class SdpOfferMessageSubscriber : IMessageSubscriber
    {
        static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public bool CanHandle(Message message) => message is SdpOfferMessage;

        public void Handle(Message message, Callback completionCallback) // signalling thread
        {
            var sdp = ((SdpOfferMessage)message).SessionDescription;
            var observer = GetRemoteSessionDescriptionObserver(message, completionCallback, sdp);

            // Step 1: SetRemoteSessionDescription
            try
            {
                message.PeerConnection.SetRemoteSessionDescription(sdp, observer);
            }
            catch(Exception ex)
            {
                completionCallback.Error($"{nameof(message.PeerConnection.SetRemoteSessionDescription)} failed: {ex.Message}");
                if(!(ex is ObjectDisposedException))
                {
                    _logger.Error(ex);
                }
            }
        }

        static Callback GetRemoteSessionDescriptionObserver(Message message, Callback completionCallback, RTCSessionDescription sdp)
            => new Callback()
                .OnError(completionCallback)
                .OnSuccess(delegate // signalling thread
                {
                    // Step 2: CreateAnswer
                    _logger.Debug($"[Negotiation Step 1/3] Remote {sdp} set for {message.PeerConnection}");
                    var createAnswerObserver = CreateAnswerObserver(message, completionCallback);
                    try
                    {
                        message.PeerConnection.CreateAnswer(createAnswerObserver);
                    }
                    catch(Exception ex)
                    {
                        completionCallback.Error($"{nameof(message.PeerConnection.CreateAnswer)} failed: {ex.Message}");
                        if(!(ex is ObjectDisposedException))
                        {
                            _logger.Error(ex);
                        }
                    }
                });

        static Callback<RTCSessionDescription> CreateAnswerObserver(Message message, Callback completionCallback)
            => new Callback<RTCSessionDescription>()
                .OnError(completionCallback)
                .OnResult(answer => // signalling thread
                {
                    // SetLocalSessionDescription() first before calling GetLocalTransceiverMetadata(),
                    // otherwise we'll get NULL transceiver mids.
                    //
                    // SetLocalSessionDescription() also generates ICE candidates, 
                    // however they are queued (by negotiation queue)
                    // and won't be send until this negotiation process completes
                    var observer = SetLocalSessionDescriptionObserver(message, completionCallback, answer);
                    try
                    {
                        message.PeerConnection.SetLocalSessionDescription(answer, observer);
                    }
                    catch(Exception ex)
                    {
                        completionCallback.Error($"{nameof(message.PeerConnection.SetLocalSessionDescription)} failed: {ex.Message}");
                        if(!(ex is ObjectDisposedException))
                        {
                            _logger.Error(ex);
                        }
                    }
                });

        static Callback SetLocalSessionDescriptionObserver(Message message, Callback completionCallback, RTCSessionDescription answer)
            => new Callback()
                .OnError(completionCallback)
                .OnSuccess(delegate // signalling thread
                {
                    // Next, send the answer along with transceiver metadata
                    try
                    {
                        var transceivers = message.PeerConnection.Room.VideoRouter.GetLocalTransceiverMetadata(message.PeerConnection);
                        message.PeerConnection.Device.EnqueueAnswer(message.PeerConnection.Id, answer, transceivers);
                        _logger.Debug($"[Negotiation Step 2/3] Answer {answer} created and sent for {message.PeerConnection}");
                    }
                    catch(Exception ex)
                    {
                        completionCallback.Error($"{nameof(IRemoteDevice.EnqueueAnswer)} failed: {ex.Message}");
                        if(!(ex is ObjectDisposedException))
                        {
                            _logger.Error(ex);
                        }
                        return;
                    }

                    // And complete the process
                    _logger.Info($"[Negotiation Step 3/3] Local description {answer} set for {message.PeerConnection}");
                    completionCallback.Success();
                });
    }
}
