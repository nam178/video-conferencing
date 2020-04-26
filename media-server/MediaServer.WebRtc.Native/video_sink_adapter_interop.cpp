#include "pch.h"

#include "passive_video_track_source.h"
#include "peer_connection_factory.h"
#include "video_sink_adapter.h"
#include "video_sink_adapter_interop.h"

VideoSinkAdapterPtr CONVENTION VideoSinkAdapterCreate(void *peer_connection_factory,
                                                      void *passive_video_track_source)
{
    auto peer_connection_factory_ptr =
        StaticCastOrThrow<Wrappers::PeerConnectionFactory>(peer_connection_factory);
    auto passive_video_track_source_ptr =
        StaticCastOrThrow<Video::PassiveVideoTrackSource>(passive_video_track_source);

    auto result = new Video::VideoSinkAdapter(passive_video_track_source_ptr);

    result->Initialise(peer_connection_factory_ptr->GetWorkerThread());

    return result;
}

void CONVENTION VideoSinkAdapterDestroy(void *video_sink_adapter_ptr)
{
    if(video_sink_adapter_ptr)
    {
        delete static_cast<Video::VideoSinkAdapter *>(video_sink_adapter_ptr);
    }
}
