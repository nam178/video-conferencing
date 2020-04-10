#pragma once

#include "pch.h"

namespace Utils
{
class StringHelper
{
  public:
    /// The API must ensure that all strings passed to the caller are
    /// null-terminated. This is a helper to ensure that calling c_str()
    /// on the given std::string will yield a null-terminated string.
    static void EnsureNullTerminatedCString(std::string &str)
    {
        if(str.empty() || (str.back() != '\0'))
        {
            str.push_back('\0');
        }
    }
};
} // namespace Utils