#include "pch.h"

#include "media_stream_track_interop.h"

void CONVENTION MediaStreamTrackDestroy(Shim::MediaStreamTrack *media_stream_track)
{
    delete media_stream_track;
}

IdString CONVENTION MediaStreamTrackId(Shim::MediaStreamTrack *media_stream_track)
{
    return media_stream_track->Id();
}

bool CONVENTION MediaStreamTrackIsAudioTrack(Shim::MediaStreamTrack *media_stream_track)
{
    return media_stream_track->IsAudioTrack();
}
