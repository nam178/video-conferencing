#include "pch.h"

#include "passive_video_track.h"

PassiveVideo::PassiveVideoTrack::PassiveVideoTrack(scoped_refptr<VideoTrackInterface> video_track)
    : _video_track(video_track)
{
}

scoped_refptr<VideoTrackInterface> PassiveVideo::PassiveVideoTrack::VideoTrack()
{
    return _video_track;
}
