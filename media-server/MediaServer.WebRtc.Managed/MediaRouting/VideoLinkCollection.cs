using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaServer.WebRtc.Managed.MediaRouting
{
    sealed class VideoLinkCollection
    {
        public void Add(VideoLink link)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<VideoLink> GetByPeerConnection(PeerConnection peerConnection)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<VideoLink> GetByVideoSource(VideoSource videoSource)
        {
            throw new NotImplementedException();
        }

        public void Remove(VideoLink link)
        {
            throw new NotImplementedException();
        }
    }

    static class VideoLinkCollectionExtensions
    {
        public static void RemoveByPeerConnection(this VideoLinkCollection collection, PeerConnection peerConnection)
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
        
        public static void RemoveByVideoSource(this VideoLinkCollection collection, VideoSource videoSource)
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
