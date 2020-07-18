#pragma once

#include "pch.h"

#include "peer_connection_factory.h"

extern "C"
{
    EXPORT MediaSources::VideoSinkAdapter *CONVENTION
    VideoSinkAdapterCreate(MediaSources::PassiveVideoTrackSource *passive_video_track_source);

    EXPORT void CONVENTION VideoSinkAdapterDestroy(MediaSources::VideoSinkAdapter *video_sink_adapter_ptr);
}