#pragma once

#include "pch.h"

#include "peer_connection_factory.h"

extern "C"
{
    EXPORT Video::VideoSinkAdapter *CONVENTION
    VideoSinkAdapterCreate(Video::PassiveVideoTrackSource *passive_video_track_source);

    EXPORT void CONVENTION VideoSinkAdapterDestroy(Video::VideoSinkAdapter *video_sink_adapter_ptr);
}