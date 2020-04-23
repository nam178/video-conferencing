#pragma once

#include "pch.h"

namespace Wrappers
{
class MediaStreamTrack
{
  public:
    MediaStreamTrack();
    virtual ~MediaStreamTrack() = default;

    // Get the id of this track.
    // This instance owns the memory for this string.
    const char *Id();

    // Whenver this track is enabled.
    // According to docs, disable track produces black/muted frames
    bool Enabled();

    // Enable or disable the track
    void Enabled(bool value);

  protected:
    virtual webrtc::MediaStreamTrackInterface *GetMediaStreamTrack() const = 0;

  private:
    std::mutex _mutex{};
    std::string _id{};
};
}; // namespace Wrappers