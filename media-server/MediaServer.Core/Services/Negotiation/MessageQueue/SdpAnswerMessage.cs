using MediaServer.Core.Models;
using MediaServer.WebRtc.Managed;
using System;

namespace MediaServer.Core.Services.Negotiation.MessageQueue
{
    sealed class SdpAnswerMessage : Message
    {
        public RTCSessionDescription SessionDescription { get; }

        public Guid OfferId { get; }

        public SdpAnswerMessage(IPeerConnection peerConnection, RTCSessionDescription desc, Guid offerId)
            : base(peerConnection)
        {
            OfferId = offerId;
            SessionDescription = desc;
        }

        public override string ToString() => $"[SdpAnswerMessage OfferId={OfferId}, Desc={SessionDescription}]";
    }
}