#pragma once

#include "pch.h"

extern "C"
{
    EXPORT Video::VideoSinkAdapter* CONVENTION
    VideoSinkAdapterCreate(void *peer_connection_factory,
                                                                 void *passive_video_track_source);

    EXPORT void CONVENTION VideoSinkAdapterDestroy(void *video_sink_adapter_ptr);
}