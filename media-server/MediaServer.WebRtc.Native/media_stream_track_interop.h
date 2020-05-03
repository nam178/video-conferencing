#pragma once

#include "pch.h"

#include "media_stream_track.h"

extern "C"
{
    using IdString = const char *;

    EXPORT void CONVENTION MediaStreamTrackDestroy(Wrappers::MediaStreamTrack *media_stream_track);

    EXPORT IdString CONVENTION MediaStreamTrackId(Wrappers::MediaStreamTrack *media_stream_track);

    EXPORT bool CONVENTION
    MediaStreamTrackIsAudioTrack(Wrappers::MediaStreamTrack *media_stream_track);
}
