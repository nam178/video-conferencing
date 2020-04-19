#pragma once

#include "pch.h"

namespace Wrappers
{
class SetRemoteSessionDescriptionObserver final
    : public rtc::RefCountedObject<webrtc::SetRemoteDescriptionObserverInterface>
{
  public:
    SetRemoteSessionDescriptionObserver(std::function<void(webrtc::RTCError)> &&callback_lambda);
    ~SetRemoteSessionDescriptionObserver();
    // Inherited via RefCountedObject
    virtual void OnSetRemoteDescriptionComplete(webrtc::RTCError error) override;

  private:
    std::function<void(webrtc::RTCError)> _callback_lambda;
};
} // namespace Wrappers