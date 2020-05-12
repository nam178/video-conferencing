namespace MediaServer.Common.Media
{
    public sealed class TransceiverMetadata
    {
        public TransceiverMetadata(string mid, MediaQuality quality, MediaKind kind)
        {
            if(string.IsNullOrWhiteSpace(mid))
                throw new System.ArgumentException("Mid cannot be NULL or empty", nameof(mid));
            TransceiverMid = mid;
            TrackQuality = quality;
            Kind = kind;
        }

        public string TransceiverMid { get; }

        public MediaQuality TrackQuality { get; }

        public MediaKind Kind { get; }
    }
}
