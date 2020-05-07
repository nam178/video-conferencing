#include "pch.h"

#include "media_stream_track.h"

Shim::MediaStreamTrack::MediaStreamTrack()
{
}

const char *Shim::MediaStreamTrack::Id()
{
    {
        // Copy the id here so we can return it to managed code
        std::lock_guard lock(_mutex);
        if(_id.empty())
        {
            auto tmp = GetMediaStreamTrack()->id();
            if(tmp.empty())
            {
                RTC_LOG(LS_ERROR) << "media stream track id is empty";
                throw new std::runtime_error("media stream track id is empty");
            }
            Utils::StringHelper::EnsureNullTerminatedCString(_id);
            _id = tmp;
        }
    }

    return _id.c_str();
}

bool Shim::MediaStreamTrack::IsAudioTrack()
{
    return GetMediaStreamTrack()->kind() == webrtc::MediaStreamTrackInterface::kAudioKind;
}

bool Shim::MediaStreamTrack::Enabled()
{
    return GetMediaStreamTrack()->enabled();
}

void Shim::MediaStreamTrack::Enabled(bool value)
{
    GetMediaStreamTrack()->set_enabled(value);
};
