#include "pch.h"

#include "api\audio_codecs\opus\audio_decoder_opus.h"
#include "noop_audio_decoder.h"
#include "noop_audio_decoder_factory.h"

std::vector<AudioCodecSpec> NoopAudio::NoopAudioDecoderFactory::GetSupportedDecoders()
{
    std::vector<AudioCodecSpec> result{};

    AudioCodecInfo opus_info{NoopAudioDecoder::sample_rate_hz,
                             NoopAudioDecoder::num_channels,
                             64 * 1024,
                             32 * 1024,
                             128 * 1024};
    opus_info.allow_comfort_noise = false;
    opus_info.supports_network_adaption = true;

    SdpAudioFormat opus_format({"opus",
                                NoopAudioDecoder::sample_rate_hz,
                                NoopAudioDecoder::num_channels,
                                {{"minptime", "10"}, {"useinbandfec", "1"}}});
    result.push_back({std::move(opus_format), opus_info});

    return result;
}

bool NoopAudio::NoopAudioDecoderFactory::IsSupportedDecoder(const SdpAudioFormat &format)
{
    auto config = AudioDecoderOpus::SdpToConfig(format);

    return config.has_value() && config->IsOk() &&
           config->num_channels == NoopAudioDecoder::num_channels;
}

std::unique_ptr<AudioDecoder> NoopAudio::NoopAudioDecoderFactory::MakeAudioDecoder(
    const SdpAudioFormat &format,
    absl::optional<AudioCodecPairId> codec_pair_id)
{
    return std::make_unique<NoopAudioDecoder>();
}
