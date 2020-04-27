using System;

namespace MediaServer.Core.Models
{
    public struct VideoSinkInfo
    {
        /// <summary>
        /// The track that this sink should be connected to
        /// </summary>
        public string ExpectedTrackId { get; set; }

        /// <summary>
        /// The track that this sink is connected to, or NULL if not connected to any track.
        /// </summary>
        public string ConnectedTrackId { get; set; }

        /// <summary>
        /// The video source that this sink is broadcasting video frames to
        /// </summary>
        public Guid VideoSourceId { get; set; }
    }
}
