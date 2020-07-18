#pragma once

#include "pch.h"

using namespace webrtc;

namespace MediaSources
{
class PassiveAudioTrackSource final : public rtc::RefCountedObject<AudioSourceInterface>
{
  public:
    void PushData(const void *audio_data,
                  int bits_per_sample,
                  int sample_rate,
                  size_t number_of_channels,
                  size_t number_of_frames);
    // Inherited via RefCountedObject
    virtual void RegisterObserver(ObserverInterface *observer) override;
    virtual void UnregisterObserver(ObserverInterface *observer) override;
    virtual SourceState state() const override;
    virtual bool remote() const override;
    virtual void AddSink(AudioTrackSinkInterface *sink) override;
    virtual void RemoveSink(AudioTrackSinkInterface *sink) override;

  private:
    std::vector<AudioTrackSinkInterface *> _sinks{};
};
} // namespace MediaSources