#include "pch.h"

#include "thread_sync.h"

Utils::ThreadSync::ThreadSync(rtc::Thread *thread, std::function<void()> &&action)
    : _thread(thread), _action(action)
{
}

void Utils::ThreadSync::Execute()
{
    _event.reset(new rtc::Event(false, false));
    _thread->Post(RTC_FROM_HERE, this);
    _event->Wait(rtc::Event::kForever);
}

void Utils::ThreadSync::OnMessage(rtc::Message *msg)
{
    try
    {
        _action();
    }
    catch(const std::exception &ex)
    {
        RTC_LOG(LS_ERROR) << "Exception occured while handling thread message: " << ex.what();
    }
    catch(...)
    {
        RTC_LOG(LS_ERROR) << "Unknown exception occured while handling thread message";
    }
    _event->Set();
}
