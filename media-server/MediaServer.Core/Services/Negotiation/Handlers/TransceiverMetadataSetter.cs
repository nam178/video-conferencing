using MediaServer.Common.Utils;
using MediaServer.Core.Common;
using MediaServer.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.Negotiation.Handlers
{
    sealed class TransceiverMetadataSetter : ITransceiverMetadataSetter
    {
        public async Task HandleAsync(IRemoteDevice device, TransceiverMetadata[] transceivers)
        {
            // Validate transceivers
            Require.NotNull(transceivers);
            if(transceivers.Any(t => string.IsNullOrEmpty(t.TransceiverMid)))
                throw new ArgumentException($"{nameof(TransceiverMetadata.TransceiverMid)} cannot be empty or null");

            // Validate device
            var data = device.GetCustomData();
            if(data.Room == null)
                throw new InvalidOperationException("Device must joined a room first");
            if(data.User == null)
                throw new UnauthorizedAccessException();

            // All good, tell the router what transceiver mid to expect.
            await data.Room.SignallingThread.ExecuteAsync(delegate
            {
                foreach(var transceiver in transceivers)
                {
                    if(string.IsNullOrWhiteSpace(transceiver.TransceiverMid))
                        throw new ArgumentException("TrackId is NULL or empty");
                    data.Room.VideoRouter.Prepare(device.Id, transceiver.TrackQuality, transceiver.Kind, transceiver.TransceiverMid);
                }
            });
        }
    }
}
