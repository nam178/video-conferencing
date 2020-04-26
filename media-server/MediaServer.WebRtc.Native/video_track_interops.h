#pragma once

#include "pch.h"

extern "C"
{
    EXPORT void CONVENTION VideoTrackAddSink(void *video_track, void *video_sink_interface);

    EXPORT void CONVENTION VideoTrackRemoveSink(void *video_track, void *video_sink_interface);
}