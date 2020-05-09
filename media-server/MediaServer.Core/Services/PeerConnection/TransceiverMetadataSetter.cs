using MediaServer.Core.Common;
using MediaServer.Models;
using System;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.PeerConnection
{
    sealed class TransceiverMetadataSetter : ITransceiverMetadataSetter
    {
        public async Task HandleAsync(IRemoteDevice device, TransceiverMetadata[] transceivers)
        {
            var data = device.GetCustomData();
            if(data.Room == null)
                throw new InvalidOperationException("Device must joined a room first");
            if(data.User == null)
                throw new UnauthorizedAccessException();

            foreach(var transceiver in transceivers)
            {
                if(string.IsNullOrWhiteSpace(transceiver.TransceiverMid))
                    throw new ArgumentException("TrackId is NULL or empty");

                await data.Room.VideoRouter.PrepareAsync(device.Id, transceiver.TrackQuality, transceiver.Kind, transceiver.TransceiverMid);
            }
        }
    }
}
