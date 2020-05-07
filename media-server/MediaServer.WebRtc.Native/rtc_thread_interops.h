#pragma once

#include "pch.h"

extern "C"
{
    using UserData = void *;
    using RtcThreadInvokeDelegate = void(CONVENTION *)(UserData);

    EXPORT void CONVENTION RtcThreadPost(Shim::RtcThread *rtc_thread,
                                         RtcThreadInvokeDelegate delegate,
                                         UserData user_data);

    EXPORT bool CONVENTION RtcThreadIsCurrent(Shim::RtcThread *rtc_thread);
}