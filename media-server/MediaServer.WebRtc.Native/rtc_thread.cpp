#include "pch.h"

#include "rtc_thread.h"
#include "thread_sync.h"

Shim::RtcThread::RtcThread(rtc::Thread *thread) : _thread(thread)
{
}

void Shim::RtcThread::Post(Callback<> &&callback)
{
    CallbackMessageData *msg_data = new CallbackMessageData(std::move(callback));
    _thread->Post(RTC_FROM_HERE, this, 0U, msg_data);
}

// Check whenever the current thread is this thread

bool Shim::RtcThread::IsCurrent()
{
    return _thread->IsCurrent();
}

void Shim::RtcThread::OnMessage(rtc::Message *msg)
{
    auto message_data = static_cast<CallbackMessageData *>(msg->pdata);
    message_data->GetCallback()();
}
