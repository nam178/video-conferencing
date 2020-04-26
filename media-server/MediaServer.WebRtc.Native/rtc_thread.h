#pragma once

#include "pch.h"

namespace Wrappers
{
class RtcThread : rtc::MessageHandler
{
  public:
    // This thread wrapper does not own the provided thread
    RtcThread(rtc::Thread *thread);

    // Post a callback to be executed on the thread
    void Post(Callback<> &&callback);

  private:
    rtc::Thread *_thread;

    // Inherited via MessageHandler
    virtual void OnMessage(rtc::Message *msg) override;

    class CallbackMessageData : public rtc::MessageData
    {
      public:
        CallbackMessageData(Wrappers::Callback<> &&callback) : _callback(callback)
        {
        }
        Wrappers::Callback<> GetCallback()
        {
            return _callback;
        }
        ~CallbackMessageData() // make sure this gets hit
        {
            ;
        }

      private:
        Wrappers::Callback<> _callback;
    };
};
} // namespace Wrappers