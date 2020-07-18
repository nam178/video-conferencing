#include "pch.h"

#include "passive_video_track_source.h"
#include "peer_connection_factory.h"
#include "video_sink_adapter.h"
#include "video_sink_adapter_interop.h"

MediaSources::VideoSinkAdapter *CONVENTION
VideoSinkAdapterCreate(MediaSources::PassiveVideoTrackSource *passive_video_track_source)
{
    return new MediaSources::VideoSinkAdapter(passive_video_track_source);
}

void CONVENTION VideoSinkAdapterDestroy(MediaSources::VideoSinkAdapter *video_sink_adapter_ptr)
{
    if(video_sink_adapter_ptr)
    {
        delete video_sink_adapter_ptr;
    }
}
