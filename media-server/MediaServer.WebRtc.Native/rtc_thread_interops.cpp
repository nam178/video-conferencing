#include "pch.h"

#include "rtc_thread.h"
#include "rtc_thread_interops.h"

void CONVENTION RtcThreadPost(RtcThreadWrapperRawPtr rtc_thread,
                              RtcThreadInvokeDelegate delegate,
                              UserData user_data)
{
    StaticCastOrThrow<Wrappers::RtcThread>(rtc_thread)
        ->Post(Wrappers::Callback<>{delegate, user_data});
}
