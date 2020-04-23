#pragma once

#include "rtc_base/logging.h"

template <class T> T *StaticCastOrThrow(void *ptr)
{
    if(!ptr)
    {
        RTC_LOG(LS_ERROR) << "null pointer to " + std::string(typeid(T).name());
        throw new std::runtime_error("null pointer to " + std::string(typeid(T).name()));
    }
    return static_cast<T *>(ptr);
}
