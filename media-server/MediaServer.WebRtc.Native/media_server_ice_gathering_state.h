#pragma once

#include "pch.h"

namespace MediaServer
{

enum class IceGatheringState : int32_t
{
    New = 0,
    Gathering = 1,
    Complete = 2,
};

IceGatheringState IceGatheringStateFrom(
    webrtc::PeerConnectionInterface::IceGatheringState ice_gathering_state)
{
    static_assert((int)IceGatheringState::New ==
                  (int)webrtc::PeerConnectionInterface::IceGatheringState::kIceGatheringNew);
    static_assert((int)IceGatheringState::Gathering ==
                  (int)webrtc::PeerConnectionInterface::IceGatheringState::kIceGatheringGathering);
    static_assert((int)IceGatheringState::Complete ==
                  (int)webrtc::PeerConnectionInterface::IceGatheringState::kIceGatheringComplete);
    return (IceGatheringState)ice_gathering_state;
}
} // namespace MediaServer