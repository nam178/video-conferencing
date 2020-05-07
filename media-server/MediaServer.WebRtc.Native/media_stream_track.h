#pragma once

#include "pch.h"

namespace Shim
{
class MediaStreamTrack
{
  public:
    MediaStreamTrack();
    virtual ~MediaStreamTrack() = default;

    // Get the id of this track.
    // This instance owns the memory for this string.
    const char *Id();

    // Whenever this is an audio track,
    // so manage code can wrap to the correct type
    bool IsAudioTrack();

    // Whenver this track is enabled.
    // According to docs, disable track produces black/muted frames
    bool Enabled();

    // Enable or disable the track
    void Enabled(bool value);

    virtual webrtc::MediaStreamTrackInterface *GetMediaStreamTrack() const = 0;

  private:
    std::mutex _mutex{};
    std::string _id{};
};
}; // namespace Shim