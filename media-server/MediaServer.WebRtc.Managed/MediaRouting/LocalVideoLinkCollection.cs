using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaServer.WebRtc.Managed.MediaRouting
{
    /// <summary>
    /// Represents one-to-many relationshop between PeerConnection and VideoSource.
    /// One PeerConnection can has many VideoSource (video streams from other peers)
    /// </summary>
    /// <remarks>Not thread safe.</remarks>
    sealed class LocalVideoLinkCollection
    {
        public void Add(LocalVideoLink link)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<LocalVideoLink> GetByPeerConnection(PeerConnection peerConnection)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<LocalVideoLink> GetByVideoSource(VideoSource videoSource)
        {
            throw new NotImplementedException();
        }

        public void Remove(LocalVideoLink link)
        {
            throw new NotImplementedException();
        }
    }

    static class VideoLinkCollectionExtensions
    {
        public static void RemoveByPeerConnection(this LocalVideoLinkCollection collection, PeerConnection peerConnection)
        {
            foreach(var link in collection.GetByPeerConnection(peerConnection).ToList())
            {
                using(link)
                {
                    link.Close();
                    collection.Remove(link);
                }
            }
        }
        
        public static void RemoveByVideoSource(this LocalVideoLinkCollection collection, VideoSource videoSource)
        {
            foreach(var link in collection.GetByVideoSource(videoSource).ToList())
            {
                using(link)
                {
                    link.Close();
                    collection.Remove(link);
                }
            }
        }
    }
}
