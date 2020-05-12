using MediaServer.Common.Media;
using MediaServer.WebRtc.Managed;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Tests.IntegrationTests
{
    public class PeerConnectionGetTransceiversTest
    {
        static PeerConnectionFactory _peerConnectionFactory = new PeerConnectionFactory();
        static PeerConnectionConfig _config = new PeerConnectionConfig();
        static RtcThreadAdapter _signallingThread;

        static PeerConnectionGetTransceiversTest()
        {
            _peerConnectionFactory.Initialize();
            _signallingThread = new RtcThreadAdapter(_peerConnectionFactory.SignallingThread);
        }

        [Fact]
        public Task GetTransceivers_NoTransceiver_ReturnsEmptyList()
        {
            return _signallingThread.ExecuteAsync(delegate
            {
                using(var peerConnectionObserver = new PeerConnectionObserver())
                using(var peerConnection = _peerConnectionFactory.CreatePeerConnection(peerConnectionObserver, _config))
                {
                    Assert.Equal(0, peerConnection.GetTransceivers().Count);
                }
            });
        }

        [Theory]
        [InlineData(MediaKind.Audio)]
        [InlineData(MediaKind.Video)]
        public Task AddTransceivers_TransceiverAddedExplicitly_ReturnsTransceiverWithCorrectMediaKind(MediaKind mediaKind)
        {
            return _signallingThread.ExecuteAsync(delegate
            {
                using(var peerConnectionObserver = new PeerConnectionObserver())
                using(var peerConnection = _peerConnectionFactory.CreatePeerConnection(peerConnectionObserver, _config))
                using(var transceiver = peerConnection.AddTransceiver(mediaKind))
                {
                    Assert.NotNull(transceiver);
                    Assert.Equal(mediaKind, transceiver.MediaKind);
                }
            });
        }

        [Fact]
        public Task GetTransceivers_TransceiversAddedViaAddTrack_ReturnsTransceiverWithCorrectMediaKind()
        {
            return _signallingThread.ExecuteAsync(delegate
            {
                using var peerConnectionObserver = new PeerConnectionObserver();
                using var peerConnection = _peerConnectionFactory.CreatePeerConnection(peerConnectionObserver, _config);
                using var passiveVideoTrackSource = new PassiveVideoTrackSource();
                using var track = _peerConnectionFactory.CreateVideoTrack("foo", passiveVideoTrackSource);
                using var sender = peerConnection.AddTrack(track, Guid.NewGuid());

                var transceivers = peerConnection.GetTransceivers();

                Assert.Equal(1, transceivers.Count);
                Assert.Equal(MediaKind.Video, transceivers[0].MediaKind);

                peerConnection.RemoveTrack(sender);
                peerConnection.Close();
            });
        }
        
        [Fact]
        public Task GetTransceivers_CalledMultipleTimes_TheMethodIsIdempotent()
        {
            return _signallingThread.ExecuteAsync(delegate
            {
                using var peerConnectionObserver = new PeerConnectionObserver();
                using var peerConnection = _peerConnectionFactory.CreatePeerConnection(peerConnectionObserver, _config);
                using var passiveVideoTrackSource = new PassiveVideoTrackSource();
                using var track1 = _peerConnectionFactory.CreateVideoTrack("foo", passiveVideoTrackSource);
                using var sender1 = peerConnection.AddTrack(track1, Guid.NewGuid());

                var transceivers1 = peerConnection.GetTransceivers();

                using var track2 = _peerConnectionFactory.CreateVideoTrack("foo", passiveVideoTrackSource);
                using var sender2 = peerConnection.AddTrack(track2, Guid.NewGuid());

                var transceivers2 = peerConnection.GetTransceivers();
                var transceivers3 = peerConnection.GetTransceivers();

                Assert.Equal(1, transceivers1.Count);
                Assert.Equal(2, transceivers2.Count);
                Assert.Equal(2, transceivers3.Count);
                Assert.Equal(MediaKind.Video, transceivers2[0].MediaKind);
                Assert.Equal(MediaKind.Video, transceivers2[1].MediaKind);

                peerConnection.RemoveTrack(sender1);
                peerConnection.RemoveTrack(sender2);
                peerConnection.Close();
            });
        }
    }
}
