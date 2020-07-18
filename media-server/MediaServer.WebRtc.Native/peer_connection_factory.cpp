#include "pch.h"

#include "noop_audio_device_module.h"
#include "noop_video_decoder_factory.h"
#include "noop_video_encoder_factory.h"
#include "noop_audio_decoder_factory.h"
#include "noop_audio_encoder_factory.h"
#include "peer_connection_factory.h"
#include "api/create_peerconnection_factory.h"

const uint8_t INIT_STATE_NONE = 0;
const uint8_t INIT_STATE_INITIALISED = 1;
const uint8_t INIT_STATE_TORN_DOWN = 2;

std::unique_ptr<rtc::Thread> CreateThread(const std::string &name,
                                          std::unique_ptr<rtc::Thread> (*factory_method)())
{
    auto tmp = factory_method();
    RTC_CHECK(tmp.get());
    tmp->SetName(name, tmp.get());
    tmp->Start();
    return tmp;
}

Shim::PeerConnectionFactory::PeerConnectionFactory()
{
}

void Shim::PeerConnectionFactory::Initialize()
{
    // Initialization guard
    uint8_t _expected{INIT_STATE_NONE};
    if(false == _initialized_state.compare_exchange_strong(_expected, INIT_STATE_INITIALISED))
    {
        throw new std::runtime_error("Already initialised");
    }

    // Initalize threads
    _network_thread =
        std::move(CreateThread("WebRTC networking thread", &rtc::Thread::CreateWithSocketServer));
    _worker_thread = std::move(CreateThread("WebRTC worker thread", &rtc::Thread::Create));
    _signalling_thread = std::move(CreateThread("WebRTC signaling thread", &rtc::Thread::Create));
    _signalling_thread_wrapper.reset(new Shim::RtcThread(_signalling_thread.get()));

    // Set log level here
    rtc::LogMessage::LogToDebug(rtc::LS_ERROR);

    // Create PeerConnectionFactory
    _peer_connection_factory = webrtc::CreatePeerConnectionFactory(
        _network_thread.get(),
        _worker_thread.get(),
        _signalling_thread.get(),
        rtc::scoped_refptr(new NoopAudio::NoopAudioDeviceModule()),
        rtc::scoped_refptr(new NoopAudio::NoopAudioEncoderFactory()),
        rtc::scoped_refptr(new NoopAudio::NoopAudioDecoderFactory()),
        std::make_unique<NoopVideo::Encoder::NoopVideoEncoderFactory>(),
        std::make_unique<NoopVideo::Decoder::NoopVideoDecoderFactory>(),
        nullptr,
        nullptr);
}

void Shim::PeerConnectionFactory::TearDown()
{
    uint8_t _expected{INIT_STATE_INITIALISED};
    if(_initialized_state.compare_exchange_strong(_expected, INIT_STATE_TORN_DOWN))
    {
        // Kill the threads
        _network_thread.reset();
        _worker_thread.reset();
        _signalling_thread.reset();
        _peer_connection_factory = nullptr;
    }
}

PeerConnectionFactoryInterface *Shim::PeerConnectionFactory::GetPeerConnectionFactory() const
{
    if(_initialized_state.load() != INIT_STATE_INITIALISED)
    {
        throw new std::runtime_error("Not Initialized");
    }
    return _peer_connection_factory.get();
}

rtc::Thread *Shim::PeerConnectionFactory::GetNetworkingThread() const
{
    return _network_thread.get();
}

rtc::Thread *Shim::PeerConnectionFactory::GetWorkerThread() const
{
    return _worker_thread.get();
}

rtc::Thread *Shim::PeerConnectionFactory::GetSignallingThread() const
{
    return _signalling_thread.get();
}

Shim::RtcThread *Shim::PeerConnectionFactory::GetSignallingThreadWrapper() const
{
    if(!_signalling_thread_wrapper)
    {
        RTC_LOG(LS_ERROR) << "Not Initialised";
        throw new std::runtime_error("Not initialised");
    }
    return _signalling_thread_wrapper.get();
}
