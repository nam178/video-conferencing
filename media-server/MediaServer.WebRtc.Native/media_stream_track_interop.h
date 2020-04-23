#pragma once

#include "pch.h"

#include "media_stream_track.h"

extern "C"
{
    using MediaStreamTrackWrapperPtr = void *;
    using IdString = const char *;

    EXPORT void CONVENTION MediaStreamDestroy(MediaStreamTrackWrapperPtr media_stream_track);

    EXPORT IdString CONVENTION MediaStreamTrackId(MediaStreamTrackWrapperPtr media_stream_track);

    EXPORT bool CONVENTION
    MediaStreamTrackIsAudioTrack(MediaStreamTrackWrapperPtr media_stream_track);
}
