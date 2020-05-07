#include "pch.h"

#include "rtc_thread.h"
#include "rtc_thread_interops.h"

void CONVENTION RtcThreadPost(Shim::RtcThread *rtc_thread,
                              RtcThreadInvokeDelegate delegate,
                              UserData user_data)
{
    rtc_thread->Post(Shim::Callback<>{delegate, user_data});
}

bool CONVENTION RtcThreadIsCurrent(Shim::RtcThread *rtc_thread)
{
    return rtc_thread->IsCurrent();
}
