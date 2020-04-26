#pragma once

#include "pch.h"

namespace Utils
{
class ThreadSync : public rtc::MessageHandler
{
  public:
    ThreadSync(rtc::Thread *thread, std::function<void()> &&action);

    void Execute();

    // Inherited via MessageHandler
    virtual void OnMessage(rtc::Message *msg) override;

  private:
    std::unique_ptr<rtc::Event> _event{};
    rtc::Thread *_thread;
    std::function<void()> _action;
};
} // namespace Utils