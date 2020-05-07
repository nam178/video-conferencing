using MediaServer.Models;
using System;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.PeerConnection
{
    sealed class SetTrackQualityRequestHandler : ISetTrackQualityRequestHandler
    {
        const int MaxTrackIdLength = 1024;

        public async Task HandleAsync(IRemoteDevice device, SetTrackQualityRequest args)
        {
            var data = device.GetCustomData();
            if(data.Room == null)
                throw new InvalidOperationException("Device must joined a room first");
            if(data.User == null)
                throw new UnauthorizedAccessException();
            if(string.IsNullOrWhiteSpace(args.TrackId))
                throw new ArgumentException("TrackId is NULL or empty");
            if(args.TrackId.Length > MaxTrackIdLength)
                throw new ArgumentException("TrackId is too long");

            await data.Room.VideoRouter.PrepareAsync(device.Id, args.TrackQuality, args.Kind, args.TrackId);
        }
    }
}
