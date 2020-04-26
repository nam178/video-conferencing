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

    class ScopedEventSetter
    {
      public:
        ScopedEventSetter(rtc::Event *event) : _event(event)
        {
        }
        ~ScopedEventSetter()
        {
            _event->Set();
        }

      private:
        rtc::Event *_event;
    };
};
} // namespace Utils