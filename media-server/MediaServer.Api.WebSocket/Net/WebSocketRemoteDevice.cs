﻿using MediaServer.Api.WebSocket.Models;
using MediaServer.Common.Media;
using MediaServer.Core.Models;
using MediaServer.WebRtc.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaServer.Api.WebSocket.Net
{
    /// <summary>
    /// Implementation of a "remote device" based on web socket connections.
    /// That is, each web socket connection is considered a remote device
    /// </summary>
    sealed class WebSocketRemoteDevice : IWebSocketRemoteDevice
    {
        readonly static ILogger _logger = NLog.LogManager.GetCurrentClassLogger();
        readonly RemoteDeviceData _customData = new RemoteDeviceData();

        public WebSocketClient WebSocketClient { get; }

        public Guid Id { get; } = Guid.NewGuid();

        public WebSocketRemoteDevice(WebSocketClient client)
        {
            WebSocketClient = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task SendAsync(string command, object args)
        {
            await WebSocketClient.SendAsync(Serialize(command, args));
            _logger.Trace($"Command {command}, Args={JsonConvert.SerializeObject(args)} sent to device {this}");
        }

        public override string ToString() => $"[WebSocketClientRemoteDevice Id={Id.ToString().Substring(0, 8)} {WebSocketClient}]";

        public void EnqueueSyncMessage(SyncMessage message)
        {
            _ = SendAsync("Sync", message);
        }

        public void EnqueueIceCandidate(Guid peerConnectionId, RTCIceCandidate candidate)
        {
            _ = SendAsync("IceCandidate", new { candidate, peerConnectionId });
        }

        public void EnqueueOffer(
            Guid peerConnectionId,
            Guid offerId,
            RTCSessionDescription description,
            IReadOnlyList<TransceiverMetadata> transceivers)
        {
            var args = new { 
                sdp = description, 
                peerConnectionId, 
                offerId, 
                transceivers = transceivers.Select(t => new WsTransceiverMetadata(t)).ToList() 
            };
            const string command = "Offer";
            _ = SendAsync(command, args);
        }

        public void EnqueueAnswer(
            Guid peerConnectionId,
            RTCSessionDescription description,
            IReadOnlyList<TransceiverMetadata> transceivers)
        {
            var args = new { 
                sdp = description, 
                peerConnectionId, 
                transceivers = transceivers.Select(t => new WsTransceiverMetadata(t)).ToList()
            };
            const string command = "Answer";
            _ = SendAsync(command, args);
        }

        public void EnqueueTransceiverMetadata(TransceiverMetadata transceiverMetadata)
        {
            _ = SendAsync("TransceiverMetadata", new WsTransceiverMetadata(transceiverMetadata));
        }

        public void Teminate() => WebSocketClient.Dispose();

        public RemoteDeviceData GetCustomData()
        {
            lock(_customData)
            {
                return new RemoteDeviceData(_customData);
            }
        }

        public void SetCustomData(RemoteDeviceData customData)
        {
            lock(_customData)
            {
                RemoteDeviceData.Copy(customData, _customData);
            }
        }

        void IDisposable.Dispose() => Teminate();

        static string Serialize(string command, object args)
        {
            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            var message = JsonConvert.SerializeObject(new CommandFormat
            {
                Command = command,
                Args = args
            }, serializerSettings);
            return message;
        }
    }
}
