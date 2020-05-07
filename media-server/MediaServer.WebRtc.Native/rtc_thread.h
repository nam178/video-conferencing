#pragma once

#include "pch.h"

namespace Shim
{
class RtcThread : rtc::MessageHandler
{
  public:
    // This thread wrapper does not own the provided thread
    RtcThread(rtc::Thread *thread);

    // Post a callback to be executed on the thread
    void Post(Callback<> &&callback);

    // Check whenever the current thread is this thread
    bool IsCurrent();

  private:
    rtc::Thread *_thread;

    // Inherited via MessageHandler
    virtual void OnMessage(rtc::Message *msg) override;

    class CallbackMessageData : public rtc::MessageData
    {
      public:
        CallbackMessageData(Shim::Callback<> &&callback) : _callback(callback)
        {
        }
        Shim::Callback<> GetCallback()
        {
            return _callback;
        }
        ~CallbackMessageData() // make sure this gets hit
        {
            ;
        }

      private:
        Shim::Callback<> _callback;
    };
};
} // namespace Shim