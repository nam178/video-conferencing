#include "pch.h"

#include "noop_audio_device_module.h"

using namespace webrtc;

int32_t NoopAudio::NoopAudioDeviceModule::ActiveAudioLayer(AudioLayer *audioLayer) const
{
    return -1;
}

int32_t NoopAudio::NoopAudioDeviceModule::RegisterAudioCallback(AudioTransport *audioCallback)
{
    return -1;
}

int32_t NoopAudio::NoopAudioDeviceModule::Init()
{
    return 0;
}

int32_t NoopAudio::NoopAudioDeviceModule::Terminate()
{
    return 0;
}

bool NoopAudio::NoopAudioDeviceModule::Initialized() const
{
    return false;
}

int16_t NoopAudio::NoopAudioDeviceModule::PlayoutDevices()
{
    return -1;
}

int16_t NoopAudio::NoopAudioDeviceModule::RecordingDevices()
{
    return -1;
}

int32_t NoopAudio::NoopAudioDeviceModule::PlayoutDeviceName(uint16_t index,
                                                            char name[kAdmMaxDeviceNameSize],
                                                            char guid[kAdmMaxGuidSize])
{
    return -1;
}

int32_t NoopAudio::NoopAudioDeviceModule::RecordingDeviceName(uint16_t index,
                                                              char name[kAdmMaxDeviceNameSize],
                                                              char guid[kAdmMaxGuidSize])
{
    return -1;
}

int32_t NoopAudio::NoopAudioDeviceModule::SetPlayoutDevice(uint16_t index)
{
    return -1;
}

int32_t NoopAudio::NoopAudioDeviceModule::SetPlayoutDevice(WindowsDeviceType device)
{
    return -1;
}

int32_t NoopAudio::NoopAudioDeviceModule::SetRecordingDevice(uint16_t index)
{
    return -1;
}

int32_t NoopAudio::NoopAudioDeviceModule::SetRecordingDevice(WindowsDeviceType device)
{
    return -1;
}

int32_t NoopAudio::NoopAudioDeviceModule::PlayoutIsAvailable(bool *available)
{
    return -1;
}

int32_t NoopAudio::NoopAudioDeviceModule::InitPlayout()
{
    return -1;
}

bool NoopAudio::NoopAudioDeviceModule::PlayoutIsInitialized() const
{
    return false;
}

int32_t NoopAudio::NoopAudioDeviceModule::RecordingIsAvailable(bool *available)
{
    return -1;
}

int32_t NoopAudio::NoopAudioDeviceModule::InitRecording()
{
    return -1;
}

bool NoopAudio::NoopAudioDeviceModule::RecordingIsInitialized() const
{
    return false;
}

int32_t NoopAudio::NoopAudioDeviceModule::StartPlayout()
{
    return -1;
}

int32_t NoopAudio::NoopAudioDeviceModule::StopPlayout()
{
    return -1;
}

bool NoopAudio::NoopAudioDeviceModule::Playing() const
{
    return false;
}

int32_t NoopAudio::NoopAudioDeviceModule::StartRecording()
{
    return -1;
}

int32_t NoopAudio::NoopAudioDeviceModule::StopRecording()
{
    return 0;
}

bool NoopAudio::NoopAudioDeviceModule::Recording() const
{
    return false;
}

int32_t NoopAudio::NoopAudioDeviceModule::InitSpeaker()
{
    return -1;
}

bool NoopAudio::NoopAudioDeviceModule::SpeakerIsInitialized() const
{
    return false;
}

int32_t NoopAudio::NoopAudioDeviceModule::InitMicrophone()
{
    return -1;
}

bool NoopAudio::NoopAudioDeviceModule::MicrophoneIsInitialized() const
{
    return false;
}

int32_t NoopAudio::NoopAudioDeviceModule::SpeakerVolumeIsAvailable(bool *available)
{
    return -1;
}

int32_t NoopAudio::NoopAudioDeviceModule::SetSpeakerVolume(uint32_t volume)
{
    return -1;
}

int32_t NoopAudio::NoopAudioDeviceModule::SpeakerVolume(uint32_t *volume) const
{
    return -1;
}

int32_t NoopAudio::NoopAudioDeviceModule::MaxSpeakerVolume(uint32_t *maxVolume) const
{
    return -1;
}

int32_t NoopAudio::NoopAudioDeviceModule::MinSpeakerVolume(uint32_t *minVolume) const
{
    return -1;
}

int32_t NoopAudio::NoopAudioDeviceModule::MicrophoneVolumeIsAvailable(bool *available)
{
    return -1;
}

int32_t NoopAudio::NoopAudioDeviceModule::SetMicrophoneVolume(uint32_t volume)
{
    return -1;
}

int32_t NoopAudio::NoopAudioDeviceModule::MicrophoneVolume(uint32_t *volume) const
{
    return -1;
}

int32_t NoopAudio::NoopAudioDeviceModule::MaxMicrophoneVolume(uint32_t *maxVolume) const
{
    return -1;
}

int32_t NoopAudio::NoopAudioDeviceModule::MinMicrophoneVolume(uint32_t *minVolume) const
{
    return -1;
}

int32_t NoopAudio::NoopAudioDeviceModule::SpeakerMuteIsAvailable(bool *available)
{
    return -1;
}

int32_t NoopAudio::NoopAudioDeviceModule::SetSpeakerMute(bool enable)
{
    return -1;
}

int32_t NoopAudio::NoopAudioDeviceModule::SpeakerMute(bool *enabled) const
{
    return -1;
}

int32_t NoopAudio::NoopAudioDeviceModule::MicrophoneMuteIsAvailable(bool *available)
{
    return -1;
}

int32_t NoopAudio::NoopAudioDeviceModule::SetMicrophoneMute(bool enable)
{
    return -1;
}

int32_t NoopAudio::NoopAudioDeviceModule::MicrophoneMute(bool *enabled) const
{
    return -1;
}

int32_t NoopAudio::NoopAudioDeviceModule::StereoPlayoutIsAvailable(bool *available) const
{
    return -1;
}

int32_t NoopAudio::NoopAudioDeviceModule::SetStereoPlayout(bool enable)
{
    return -1;
}

int32_t NoopAudio::NoopAudioDeviceModule::StereoPlayout(bool *enabled) const
{
    return -1;
}

int32_t NoopAudio::NoopAudioDeviceModule::StereoRecordingIsAvailable(bool *available) const
{
    return -1;
}

int32_t NoopAudio::NoopAudioDeviceModule::SetStereoRecording(bool enable)
{
    return -1;
}

int32_t NoopAudio::NoopAudioDeviceModule::StereoRecording(bool *enabled) const
{
    return -1;
}

int32_t NoopAudio::NoopAudioDeviceModule::PlayoutDelay(uint16_t *delayMS) const
{
    return 0;
}

bool NoopAudio::NoopAudioDeviceModule::BuiltInAECIsAvailable() const
{
    return false;
}

bool NoopAudio::NoopAudioDeviceModule::BuiltInAGCIsAvailable() const
{
    return false;
}

bool NoopAudio::NoopAudioDeviceModule::BuiltInNSIsAvailable() const
{
    return false;
}

int32_t NoopAudio::NoopAudioDeviceModule::EnableBuiltInAEC(bool enable)
{
    return -1;
}

int32_t NoopAudio::NoopAudioDeviceModule::EnableBuiltInAGC(bool enable)
{
    return -1;
}

int32_t NoopAudio::NoopAudioDeviceModule::EnableBuiltInNS(bool enable)
{
    return -1;
}
