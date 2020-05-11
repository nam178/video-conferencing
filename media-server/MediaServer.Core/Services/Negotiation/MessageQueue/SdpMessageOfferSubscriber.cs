﻿using MediaServer.Common.Patterns;
using MediaServer.Models;
using MediaServer.WebRtc.Managed;
using NLog;
using System;

namespace MediaServer.Core.Services.Negotiation.MessageQueue
{
    sealed class SdpMessageOfferSubscriber : IMessageSubscriber
    {
        static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public bool CanHandle(Message message)
        {
            return message is SdpMessage
                && "offer".Equals(
                    ((SdpMessage)message).SessionDescription.Type,
                    StringComparison.InvariantCultureIgnoreCase);
        }

        public void Handle(Message message, Observer completionCallback) // signalling thread
        {
            var sdp = ((SdpMessage)message).SessionDescription;
            var observer = GetRemoteSessionDescriptionObserver(message, completionCallback, sdp);

            // Step 1: SetRemoteSessionDescription
            try
            {
                message.PeerConnection.SetRemoteSessionDescription(sdp, observer);
            }
            catch(Exception ex)
            {
                completionCallback.Error($"{nameof(message.PeerConnection.SetRemoteSessionDescription)} failed: {ex.Message}");
            }

        }

        static Observer GetRemoteSessionDescriptionObserver(Message message, Observer completionCallback, RTCSessionDescription sdp)
            => new Observer()
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
                    }
                });

        static Observer<RTCSessionDescription> CreateAnswerObserver(Message message, Observer completionCallback)
            => new Observer<RTCSessionDescription>()
                .OnError(completionCallback)
                .OnResult(answer => // signalling thread
                {
                    // Step 3: Send Answer + SetLocalSessionDescription
                    try
                    {
                        message.PeerConnection.Device.EnqueueSessionDescription(message.PeerConnection.Id, answer);
                        _logger.Debug($"[Negotiation Step 2/3] Answer {answer} created and sent for {message.PeerConnection}");
                    }
                    catch(Exception ex)
                    {
                        completionCallback.Error($"{nameof(IRemoteDevice.EnqueueSessionDescription)} failed: {ex.Message}");
                        return;
                    }

                    // SetLocalSessionDescriptionAsync() must be after EnqueueSessionDescription()
                    // because it SetLocalSessionDescriptionAsync() generates ICE candidates,
                    // and we want to send ICE candidates after remote SDP is set.
                    var observer = SetLocalSessionDescriptionObserver(message, completionCallback, answer);
                    try
                    {
                        message.PeerConnection.SetLocalSessionDescription(answer, observer);
                    }
                    catch(Exception ex)
                    {
                        completionCallback.Error($"{nameof(message.PeerConnection.SetLocalSessionDescription)} failed: {ex.Message}");
                    }
                });

        static Observer SetLocalSessionDescriptionObserver(Message message, Observer completionCallback, RTCSessionDescription answer)
            => new Observer()
                .OnError(completionCallback)
                .OnSuccess(delegate // signalling thread
                {
                    _logger.Info($"[Negotiation Step 3/3] Local description {answer} set for {message.PeerConnection}");
                    completionCallback.Success();
                });
    }
}
