#pragma once

#include "pch.h"

#include "rtc_thread.h"

using namespace webrtc;
using namespace rtc;

namespace Shim
{
// Responsible for the setup and tear down of the PeerConnectionFactory.
// Designed to be used one per room.
class PeerConnectionFactory final
{
  public:
    PeerConnectionFactory();

    // The instance must be initialised before using.
    // Not thread safe, Initialise() and TearDown() must be called in order.
    void Initialize();

    // Shut down the internal threads,etc..
    // Must be called to destroy this instance
    // Not thread safe, Initialise() and TearDown() must be called in order.
    void TearDown();

    // After initialisation, PeerConnectionInterface and threads will be accessible.
    PeerConnectionFactoryInterface *GetPeerConnectionFactory() const;

    rtc::Thread *GetNetworkingThread() const;
    rtc::Thread *GetWorkerThread() const;
    rtc::Thread *GetSignallingThread() const;
    Shim::RtcThread *GetSignallingThreadWrapper() const;

  private:
    std::atomic<uint8_t> _initialized_state = {0};
    std::unique_ptr<rtc::Thread> _network_thread;
    std::unique_ptr<rtc::Thread> _signalling_thread;
    std::unique_ptr<rtc::Thread> _worker_thread;
    std::unique_ptr<Shim::RtcThread> _signalling_thread_wrapper{};

    scoped_refptr<PeerConnectionFactoryInterface> _peer_connection_factory = {};
};
} // namespace Shim