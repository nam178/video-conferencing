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
    ScopedEventSetter(_event.get());
    _action();
}
