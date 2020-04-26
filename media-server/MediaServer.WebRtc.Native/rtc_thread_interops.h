#pragma once

#include "pch.h"

extern "C"
{
    using RtcThreadWrapperRawPtr = void *;
    using UserData = void *;
    using RtcThreadInvokeDelegate = void(CONVENTION *)(UserData);

    EXPORT void CONVENTION RtcThreadPost(RtcThreadWrapperRawPtr rtc_thread,
                                         RtcThreadInvokeDelegate delegate,
                                         UserData user_data);
}