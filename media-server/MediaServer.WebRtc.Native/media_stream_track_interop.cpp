#include "pch.h"

#include "media_stream_track_interop.h"

void CONVENTION MediaStreamTrackDestroy(Wrappers::MediaStreamTrack *media_stream_track)
{
    delete media_stream_track;
}

IdString CONVENTION MediaStreamTrackId(Wrappers::MediaStreamTrack *media_stream_track)
{
    return media_stream_track->Id();
}

bool CONVENTION MediaStreamTrackIsAudioTrack(Wrappers::MediaStreamTrack *media_stream_track)
{
    return media_stream_track->IsAudioTrack();
}
