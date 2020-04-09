using System;
using System.Collections.Generic;

namespace Microsoft.MixedReality.WebRTC
{
    public sealed class PassiveVideoTrackSource : IDisposable
    {
        readonly List<PassiveVideoTrack> _tracks = new List<PassiveVideoTrack>();

        internal PassiveVideoTrackSourceSafeHandle Native { get; }

        public PassiveVideoTrackSource()
        {
            Native = new PassiveVideoTrackSourceSafeHandle();
        }

        public void PushVideoFrame(IntPtr videoFrame) => PassiveVideoTrackSourceInterop.PushVideoFrame(Native, videoFrame);

        public void Dispose()
        {
            lock(_tracks)
            {
                if(_tracks.Count > 0)
                {
                    throw new InvalidOperationException($"Cannot dispose the source - still being used by tracks");
                }
            }
            Native.Dispose();
        }

        /// <summary>
        /// Called when this source is added to any given track
        /// </summary>
        /// <param name="videoTrack"></param>
        internal void AddedToVideoTrack(PassiveVideoTrack videoTrack)
        {
            if(videoTrack is null)
                throw new ArgumentNullException(nameof(videoTrack));
            lock(_tracks)
            {
                if(_tracks.Contains(videoTrack))
                {
                    throw new InvalidOperationException($"Already added to the track");
                }
                _tracks.Add(videoTrack);
            }
        }
    }
}


