#pragma once

#include <exception>

namespace NoopVideo
{
class NotSupportedException : public std::exception
{
  public:
    NotSupportedException()
    {
    
    }

    NotSupportedException(const char *const message) : exception(message)
    {
    }
};
}
