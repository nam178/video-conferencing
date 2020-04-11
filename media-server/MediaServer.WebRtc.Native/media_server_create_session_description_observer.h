#pragma once

#include "pch.h"

namespace MediaServer
{
// Generic observer, allows you to supply a callback.
// Designed so it owns it self, self-delete after the callback is called
class CreateSessionDescriptionObserver final
    : public rtc::RefCountedObject<webrtc::CreateSessionDescriptionObserver>
{
  public:
    // Constructor from a callback
    CreateSessionDescriptionObserver(
        Callback<Result<webrtc::SessionDescriptionInterface *>> &&callback);

    // Inherited via RefCountedObject
    void OnSuccess(webrtc::SessionDescriptionInterface *desc) override;
    void OnFailure(webrtc::RTCError error) override;

  private:
    Callback<Result<webrtc::SessionDescriptionInterface *>> _callback;
};
} // namespace MediaServer