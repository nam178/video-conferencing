#pragma once

#include "pch.h"

#include "rtp_transceiver.h"

using CreateUnmanagedInstanceFuncPtr = void *(CONVENTION *)(Shim::RtpTransceiver *);
