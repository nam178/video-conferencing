// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma once

#include <cassert>
#include <cstdint>
#include <functional>
#include <mutex>
#include <string>
#include <string_view>
#include <unordered_set>
#include <atomic>
#include <memory>

// Common stuff
#include "call_back.h"
#include "string_helper.h"

#include "result.h"

#if defined(MR_SHARING_WIN)

// #include "microsoft/targetver.h"

#define WEBRTC_WIN 1

#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif

#ifndef NOMINMAX
#define NOMINMAX
#endif

#include <windows.h>

#elif defined(MR_SHARING_ANDROID)

#define WEBRTC_POSIX 1
#define WEBRTC_ANDROID 1

#endif

// Prevent external headers from triggering warnings that would break compiling
// due to warning-as-error.
#pragma warning(push, 2)
#pragma warning(disable : 4100)
#pragma warning(disable : 4244)

// Core WebRTC
#include "absl/memory/memory.h"
#include "api/audio_codecs/builtin_audio_decoder_factory.h"
#include "api/audio_codecs/builtin_audio_encoder_factory.h"
#include "api/data_channel_interface.h"
#include "api/media_stream_interface.h"
#include "api/peer_connection_interface.h"
#include "api/rtp_sender_interface.h"
#include "api/transport/bitrate_settings.h"
#include "api/video/i420_buffer.h"
#include "media/engine/internal_decoder_factory.h"
#include "media/engine/internal_encoder_factory.h"
#include "media/engine/multiplex_codec_factory.h"
#include "modules/audio_device/include/audio_device.h"
#include "modules/audio_processing/include/audio_processing.h"
#include "modules/video_capture/video_capture_factory.h"
#include "rtc_base/memory/aligned_malloc.h"

// libyuv from WebRTC repository for color conversion
#include "libyuv.h"

#include "not_supported_exception.h"

#pragma warning(pop)
