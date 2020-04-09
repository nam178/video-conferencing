#pragma once

#include "pch.h"

using namespace webrtc;
using namespace rtc;

namespace PassiveVideo
{
class PassiveVideoTrackSource : public rtc::RefCountedObject<webrtc::VideoTrackSourceInterface>
{
  public:
    PassiveVideoTrackSource();

    // Push a frame into this source
    // Must be invoked on worker thread
    void PushVideoFrame(const VideoFrame &frame);

    // Inherited via RefCountedObject
    virtual void RegisterObserver(ObserverInterface *observer) override;
    virtual void UnregisterObserver(ObserverInterface *observer) override;
    virtual SourceState state() const override;
    virtual bool remote() const override;
    virtual void AddOrUpdateSink(VideoSinkInterface<VideoFrame> *sink,
                                 const VideoSinkWants &wants) override;
    virtual void RemoveSink(VideoSinkInterface<VideoFrame> *sink) override;
    virtual bool is_screencast() const override;
    virtual absl::optional<bool> needs_denoising() const override;
    virtual bool GetStats(Stats *stats) override;

  private:
    std::vector<VideoSinkInterface<VideoFrame> *> _sinks;
    uint32_t _total = 0;
};
} // namespace PassiveVideo
