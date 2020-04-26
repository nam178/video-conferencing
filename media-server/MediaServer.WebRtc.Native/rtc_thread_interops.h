#pragma once

#include "pch.h"

extern "C"
{
    using RtcThreadRawPtr = void *;
    using UserData = void *;
    using RtcThreadInvokeDelegate = void(CONVENTION *)(UserData);

    EXPORT void CONVENTION RtcThreadPost(RtcThreadRawPtr rtc_thread,
                                         RtcThreadInvokeDelegate delegate,
                                         UserData user_data);
}