// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma once

// P/Invoke uses stdcall by default. This can be changed, but Unity's IL2CPP
// does not understand the CallingConvention attribute and instead
// unconditionally forces stdcall. So use stdcall in the API to be compatible.
#if defined(MR_SHARING_WIN)
#define EXPORT __declspec(dllexport)
#define CONVENTION __stdcall
#elif defined(MR_SHARING_ANDROID)
#define EXPORT __attribute__((visibility("default")))
#define CONVENTION
#else
#error Unknown platform, see export.h
#endif
