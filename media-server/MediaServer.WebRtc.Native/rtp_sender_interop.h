#pragma once

#include "pch.h"

#include "rtp_sender.h"

extern "C"
{
    EXPORT void CONVENTION RtpSenderDestroy(Wrappers::RtpSender *rtp_sender);
};