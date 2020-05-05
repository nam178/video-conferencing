﻿using MediaServer.Models;
using MediaServer.WebRtc.Managed;
using System;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.PeerConnection
{
    public interface IOfferHandler
    {
        Task HandleAsync(IRemoteDevice remoteDevice, RTCSessionDescription request);
    }
}
