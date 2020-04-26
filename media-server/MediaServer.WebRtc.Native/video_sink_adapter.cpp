#include "pch.h"

#include "thread_sync.h"
#include "video_sink_adapter.h"

Video::VideoSinkAdapter::VideoSinkAdapter(Video::PassiveVideoTrackSource *video_track_source)
    : _video_track_source(video_track_source)
{
}

void Video::VideoSinkAdapter::Initialise(rtc::Thread *worker_thread)
{
    if(!worker_thread)
    {
        RTC_LOG(LS_ERROR) << "worker_thread is nullptr";
        throw new std::runtime_error("worker_thread is nullptr");
    }

    // We'll call AddOrUpdateSink(), but ensure it is called on the worker thread
    auto tmp = _video_track_source;
    Utils::ThreadSync(worker_thread, [tmp, this]() {
        rtc::VideoSinkWants video_sink_wants{};
        tmp->AddOrUpdateSink(this, video_sink_wants);
    }).Execute();
}

void Video::VideoSinkAdapter::OnFrame(const webrtc::VideoFrame &frame)
{
    _video_track_source->PushVideoFrame(frame);
}
