#pragma once

#include "pch.h"

#include "media_stream_track.h"

extern "C"
{
    using IdString = const char *;

    EXPORT void CONVENTION MediaStreamTrackDestroy(Shim::MediaStreamTrack *media_stream_track);

    EXPORT IdString CONVENTION MediaStreamTrackId(Shim::MediaStreamTrack *media_stream_track);

    EXPORT bool CONVENTION
    MediaStreamTrackIsAudioTrack(Shim::MediaStreamTrack *media_stream_track);
}
