#pragma once

#include "pch.h"

using namespace webrtc;

namespace NoopAudio
{
class NoopAudioDeviceModule : public rtc::RefCountedObject<AudioDeviceModule>
{
    // Inherited via RefCountedObject
    virtual int32_t ActiveAudioLayer(AudioLayer *audioLayer) const override;
    virtual int32_t RegisterAudioCallback(AudioTransport *audioCallback) override;
    virtual int32_t Init() override;
    virtual int32_t Terminate() override;
    virtual bool Initialized() const override;
    virtual int16_t PlayoutDevices() override;
    virtual int16_t RecordingDevices() override;
    virtual int32_t PlayoutDeviceName(uint16_t index,
                                      char name[kAdmMaxDeviceNameSize],
                                      char guid[kAdmMaxGuidSize]) override;
    virtual int32_t RecordingDeviceName(uint16_t index,
                                        char name[kAdmMaxDeviceNameSize],
                                        char guid[kAdmMaxGuidSize]) override;
    virtual int32_t SetPlayoutDevice(uint16_t index) override;
    virtual int32_t SetPlayoutDevice(WindowsDeviceType device) override;
    virtual int32_t SetRecordingDevice(uint16_t index) override;
    virtual int32_t SetRecordingDevice(WindowsDeviceType device) override;
    virtual int32_t PlayoutIsAvailable(bool *available) override;
    virtual int32_t InitPlayout() override;
    virtual bool PlayoutIsInitialized() const override;
    virtual int32_t RecordingIsAvailable(bool *available) override;
    virtual int32_t InitRecording() override;
    virtual bool RecordingIsInitialized() const override;
    virtual int32_t StartPlayout() override;
    virtual int32_t StopPlayout() override;
    virtual bool Playing() const override;
    virtual int32_t StartRecording() override;
    virtual int32_t StopRecording() override;
    virtual bool Recording() const override;
    virtual int32_t InitSpeaker() override;
    virtual bool SpeakerIsInitialized() const override;
    virtual int32_t InitMicrophone() override;
    virtual bool MicrophoneIsInitialized() const override;
    virtual int32_t SpeakerVolumeIsAvailable(bool *available) override;
    virtual int32_t SetSpeakerVolume(uint32_t volume) override;
    virtual int32_t SpeakerVolume(uint32_t *volume) const override;
    virtual int32_t MaxSpeakerVolume(uint32_t *maxVolume) const override;
    virtual int32_t MinSpeakerVolume(uint32_t *minVolume) const override;
    virtual int32_t MicrophoneVolumeIsAvailable(bool *available) override;
    virtual int32_t SetMicrophoneVolume(uint32_t volume) override;
    virtual int32_t MicrophoneVolume(uint32_t *volume) const override;
    virtual int32_t MaxMicrophoneVolume(uint32_t *maxVolume) const override;
    virtual int32_t MinMicrophoneVolume(uint32_t *minVolume) const override;
    virtual int32_t SpeakerMuteIsAvailable(bool *available) override;
    virtual int32_t SetSpeakerMute(bool enable) override;
    virtual int32_t SpeakerMute(bool *enabled) const override;
    virtual int32_t MicrophoneMuteIsAvailable(bool *available) override;
    virtual int32_t SetMicrophoneMute(bool enable) override;
    virtual int32_t MicrophoneMute(bool *enabled) const override;
    virtual int32_t StereoPlayoutIsAvailable(bool *available) const override;
    virtual int32_t SetStereoPlayout(bool enable) override;
    virtual int32_t StereoPlayout(bool *enabled) const override;
    virtual int32_t StereoRecordingIsAvailable(bool *available) const override;
    virtual int32_t SetStereoRecording(bool enable) override;
    virtual int32_t StereoRecording(bool *enabled) const override;
    virtual int32_t PlayoutDelay(uint16_t *delayMS) const override;
    virtual bool BuiltInAECIsAvailable() const override;
    virtual bool BuiltInAGCIsAvailable() const override;
    virtual bool BuiltInNSIsAvailable() const override;
    virtual int32_t EnableBuiltInAEC(bool enable) override;
    virtual int32_t EnableBuiltInAGC(bool enable) override;
    virtual int32_t EnableBuiltInNS(bool enable) override;
};
} // namespace NoopAudio