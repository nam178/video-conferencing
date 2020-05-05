#pragma once

#include "pch.h"

namespace Wrappers
{
// Generic observer, allows you to supply a callback.
class CreateSessionDescriptionObserver final
    : public rtc::RefCountedObject<webrtc::CreateSessionDescriptionObserver>
{
  public:
    // Constructor from a callback
    CreateSessionDescriptionObserver(
        std::function<void(Result<webrtc::SessionDescriptionInterface *>)> &&callback_lambda);

    // Inherited via RefCountedObject
    void OnSuccess(webrtc::SessionDescriptionInterface *desc) override;
    void OnFailure(webrtc::RTCError error) override;

  private:
    std::function<void(Result<webrtc::SessionDescriptionInterface *>)> _callback_lambda;
};
} // namespace Wrappers