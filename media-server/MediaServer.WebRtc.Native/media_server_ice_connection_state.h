#pragma once

#include "pch.h"

namespace MediaServer
{
enum class IceConnectionState : int32_t
{
    IceConnectionNew = 0,
    IceConnectionChecking = 1,
    IceConnectionConnected = 2,
    IceConnectionCompleted = 3,
    IceConnectionFailed = 4,
    IceConnectionDisconnected = 5,
    IceConnectionClosed = 6,
    IceConnectionMax = 7
};

IceConnectionState IceConnectionStateFrom(
    webrtc::PeerConnectionInterface::IceConnectionState ice_connection_state)
{
    static_assert((int)IceConnectionState::IceConnectionNew ==
                  (int)webrtc::PeerConnectionInterface::IceConnectionState::kIceConnectionNew);
    static_assert((int)IceConnectionState::IceConnectionChecking ==
                  (int)webrtc::PeerConnectionInterface::IceConnectionState::kIceConnectionChecking);
    static_assert((int)IceConnectionState::IceConnectionClosed ==
                  (int)webrtc::PeerConnectionInterface::IceConnectionState::kIceConnectionClosed);
    static_assert(
        (int)IceConnectionState::IceConnectionCompleted ==
        (int)webrtc::PeerConnectionInterface::IceConnectionState::kIceConnectionCompleted);
    static_assert(
        (int)IceConnectionState::IceConnectionConnected ==
        (int)webrtc::PeerConnectionInterface::IceConnectionState::kIceConnectionConnected);
    static_assert(
        (int)IceConnectionState::IceConnectionDisconnected ==
        (int)webrtc::PeerConnectionInterface::IceConnectionState::kIceConnectionDisconnected);
    static_assert((int)IceConnectionState::IceConnectionFailed ==
                  (int)webrtc::PeerConnectionInterface::IceConnectionState::kIceConnectionFailed);
    static_assert((int)IceConnectionState::IceConnectionMax ==
                  (int)webrtc::PeerConnectionInterface::IceConnectionState::kIceConnectionMax);
    return (IceConnectionState)ice_connection_state;
}
} // namespace MediaServer