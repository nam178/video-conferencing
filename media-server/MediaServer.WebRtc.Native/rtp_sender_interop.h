#pragma once

#include "pch.h"

#include "rtp_sender.h"

extern "C"
{
    EXPORT void CONVENTION RtpSenderDestroy(Shim::RtpSender *rtp_sender);

    EXPORT Shim::MediaStreamTrack *CONVENTION RtpSenderGetTrack(Shim::RtpSender *rtp_sender);

    EXPORT void CONVENTION RtpSenderSetTrack(Shim::RtpSender *rtp_sender,
                                             Shim::MediaStreamTrack *track);

    EXPORT void CONVENTION RtpSenderSetStreamId(Shim::RtpSender *rtp_sender, const char *stream_id);

    EXPORT const char* CONVENTION RtpSenderGetStreamId(Shim::RtpSender *rtp_sender);
};