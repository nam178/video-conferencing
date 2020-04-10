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

#if defined(MR_SHARING_WIN)

#include "microsoft/targetver.h"

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
#include "api/datachannelinterface.h"
// #include "api/mediaconstraintsinterface.h"
#include "api/mediastreaminterface.h"
#include "api/peerconnectioninterface.h"
#include "api/rtpsenderinterface.h"
#include "api/transport/bitrate_settings.h"
#include "api/video/i420_buffer.h"
// #include "api/videotracksourceproxy.h"
#include "media/engine/internaldecoderfactory.h"
#include "media/engine/internalencoderfactory.h"
#include "media/engine/multiplexcodecfactory.h"
// #include "media/engine/webrtcvideocapturerfactory.h"
// #include "media/engine/webrtcvideodecoderfactory.h"
// #include "media/engine/webrtcvideoencoderfactory.h"
#include "modules/audio_device/include/audio_device.h"
#include "modules/audio_processing/include/audio_processing.h"
#include "modules/video_capture/video_capture_factory.h"
#include "rtc_base/memory/aligned_malloc.h"

// libyuv from WebRTC repository for color conversion
#include "libyuv.h"

#include "not_supported_exception.h"

// UWP wrappers
#if defined(WINUWP)

// Stop WinRT from polluting the global namespace
// https://developercommunity.visualstudio.com/content/problem/859178/asyncinfoh-defines-the-error-symbol-at-global-name.html
#define _HIDE_GLOBAL_ASYNC_STATUS 1

#include <winrt/windows.applicationmodel.core.h>

#include "impl_org_webRtc_EventQueue.h"
#include "impl_org_webRtc_VideoCapturer.h"
#include "impl_org_webRtc_VideoCapturerCreationParameters.h"
#include "impl_org_webRtc_VideoDeviceInfo.h"
#include "impl_org_webRtc_VideoFormat.h"
#include "impl_org_webRtc_WebRtcFactory.h"
#include "impl_org_webRtc_WebRtcFactoryConfiguration.h"
#include "impl_org_webRtc_WebRtcLib.h"
#include "impl_org_webRtc_WebRtcLibConfiguration.h"

#endif  // defined(WINUWP)

#pragma warning(pop)
