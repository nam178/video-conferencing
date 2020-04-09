#include "pch.h"

#include "passive_video_track.h"

PassiveVideo::PassiveVideoTrack::PassiveVideoTrack(
    const std::string &label,
    PassiveVideoTrackSource *passive_video_track_source,
    RefPtr<GlobalFactory> global_factory)
    : _global_factory(global_factory),
      _video_track(nullptr),
      _label(label),
      _video_track_source(passive_video_track_source)
{
    if(!_global_factory)
    {
        throw new std::runtime_error("Failed to accquire peer_connection_factory");
    }

    auto peer_connection_factory = _global_factory->GetPeerConnectionFactory();
    if(!peer_connection_factory)
        throw new std::runtime_error("Failed to get PeerConnectionFactory");

    // Create native VideoTrack
    _video_track =
        std::move(peer_connection_factory->CreateVideoTrack(_label, _video_track_source));
}

scoped_refptr<VideoTrackInterface> PassiveVideo::PassiveVideoTrack::VideoTrack()
{
    return _video_track;
}
