#pragma once

#include <string>

template <class T> struct Result
{
    T _result{};
    bool _success = false;
    std::string _error_message{};
};