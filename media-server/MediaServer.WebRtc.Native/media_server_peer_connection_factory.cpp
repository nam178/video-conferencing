#include "pch.h"

#include "media_server_peer_connection_factory.h"
#include "noop_video_decoder_factory.h"
#include "noop_video_encoder_factory.h"

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

void MediaServer::PeerConnectionFactory::Initialize()
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

    // Create PeerConnectionFactory
    _peer_connection_factory = webrtc::CreatePeerConnectionFactory(
        _network_thread.get(),
        _worker_thread.get(),
        _signalling_thread.get(),
        nullptr,
        webrtc::CreateBuiltinAudioEncoderFactory(),
        webrtc::CreateBuiltinAudioDecoderFactory(),
        std::make_unique<NoopVideo::Encoder::NoopVideoEncoderFactory>(),
        std::make_unique<NoopVideo::Decoder::NoopVideoDecoderFactory>(),
        nullptr,
        nullptr);
}

void MediaServer::PeerConnectionFactory::TearDown()
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

PeerConnectionFactoryInterface *MediaServer::PeerConnectionFactory::
    GetPeerConnectionFactory()
{
    if(_initialized_state.load() != INIT_STATE_INITIALISED)
    {
        throw new std::runtime_error("Not Initialized");
    }
    return _peer_connection_factory.get();
}
