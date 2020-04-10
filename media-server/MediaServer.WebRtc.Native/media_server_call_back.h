#pragma once

#include "export.h"
#include "pch.h"

namespace MediaServer
{
template <typename... Args> struct Callback
{
    using CallbackFunctionPtr = void(CONVENTION *)(void *, Args...);

    CallbackFunctionPtr _callback_function_ptr{};

    void *_user_data{};

    constexpr explicit operator bool() const noexcept
    {
        return _callback_function_ptr != nullptr;
    }

    constexpr void operator()(Args... args) const noexcept
    {
        if(_callback_function_ptr)
        {
            _callback_function_ptr(_user_data, std::forward<Args>(args)...);
        }
    }
};
} // namespace MediaServer