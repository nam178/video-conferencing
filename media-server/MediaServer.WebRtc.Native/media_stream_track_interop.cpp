#include "pch.h"

#include "media_stream_track_interop.h"

void CONVENTION MediaStreamTrackDestroy(MediaStreamTrackWrapperPtr media_stream_track)
{
    auto tmp = static_cast<Wrappers::MediaStreamTrack *>(media_stream_track);
    if(tmp)
    {
        delete tmp;
    }
}

IdString CONVENTION MediaStreamTrackId(MediaStreamTrackWrapperPtr media_stream_track)
{
    return StaticCastOrThrow<Wrappers::MediaStreamTrack>(media_stream_track)->Id();
}

bool CONVENTION MediaStreamTrackIsAudioTrack(MediaStreamTrackWrapperPtr media_stream_track)
{
    return StaticCastOrThrow<Wrappers::MediaStreamTrack>(media_stream_track)->IsAudioTrack();
}
