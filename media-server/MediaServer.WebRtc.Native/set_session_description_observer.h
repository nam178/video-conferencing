#pragma once

#include "pch.h"

using namespace webrtc;

namespace Wrappers
{
using Success = bool;
using ErrorMessage = const char *;

class SetRemoteSessionDescriptionObserver final
    : public rtc::RefCountedObject<webrtc::SetSessionDescriptionObserver>
{
  public:
    SetRemoteSessionDescriptionObserver(
        std::function<void(Success, ErrorMessage)> &&callback_lambda);

    // implements webrtc::SetSessionDescriptionObserver
    virtual void OnSuccess() override;
    virtual void OnFailure(RTCError error) override;
    virtual void OnFailure(const std::string &error) override;

  private:
    std::function<void(Success, ErrorMessage)> _callback_lambda;
};
} // namespace Wrappers