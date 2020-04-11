using MediaServer.Common.Utils;
using MediaServer.Models;
using MediaServer.WebRtc.Managed;
using Microsoft.Extensions.Hosting;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MediaServer.WebRtc
{
    sealed class PeerManager : IHostedService
    {
        readonly ILogger _logger = NLog.LogManager.GetCurrentClassLogger();
        readonly Room _room;
        PassiveVideoTrack _passiveVideoTrack;
        PassiveVideoTrackSource _passiveVideoTrackSource;

        public PeerManager(Room room)
        {
            _room = room ?? throw new ArgumentNullException(nameof(room));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Start();
            return Task.CompletedTask;
        }

        PeerConnectionFactory _peerConnectionFactory;
        PeerConnection _peerConnection;

        async void Start()
        {
            try
            {
                _room.PeerAdded += (object room, EventArgs<Peer> peerEventArgs) =>
                {
                    _room.DispatchQueue.ExecuteAsync(async delegate
                    {
                        try
                        {
                            await peerEventArgs.Payload.Signaller.SendMessageAsync("Ready");
                        }
                        catch(Exception ex)
                        {
                            _logger.Error(ex);
                            throw new NotImplementedException("TODO handle error, removing this peer from the room to disconnect him");
                        }
                    });
                };

                // Create a new peer connection automatically disposed at the end of the program

                var observer = new PeerConnectionObserver();

                void Observer_IceCandidateAdded(object? sender, EventArgs<Managed.IceCandidate> e)
                {
                    _room.DispatchQueue.ExecuteAsync(async delegate
                    {
                        try
                        {
                            await WaitForPeers();
                            await _room.Peers.First().Signaller.SendAsync("SetIceCandidate", new Models.IceCandidate
                            {
                                Candidate = e.Payload.Sdp,
                                SdpMid = e.Payload.SdpMid,
                                SdpMLineIndex = e.Payload.MLineIndex
                            });
                        }
                        catch(Exception ex)
                        {
                            _logger.Fatal(ex);
                            throw new NotImplementedException("TODO: Handle fatal error - need to remove peer from the room");
                        }
                    });
                }

                void Observer_IceConnectionStateChanged(object? sender, EventArgs<IceConnectionState> e)
                {
                    switch(e.Payload)
                    {
                        case IceConnectionState.New:
                        case IceConnectionState.Checking:
                            _logger.Trace($"Ice state: {e.Payload}");
                            break;
                        case IceConnectionState.Connected:
                        case IceConnectionState.Completed:
                            _logger.Info($"Ice state: {e.Payload}");
                            break;
                        case IceConnectionState.Failed:
                        case IceConnectionState.Disconnected:
                        case IceConnectionState.Closed:
                        case IceConnectionState.Max:
                            _logger.Warn($"Ice state: {e.Payload}");
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                observer.IceCandidateAdded += Observer_IceCandidateAdded;
                observer.IceConnectionStateChanged += Observer_IceConnectionStateChanged;


                _peerConnectionFactory = new PeerConnectionFactory();
                _peerConnectionFactory.Initialize();
                _peerConnection = _peerConnectionFactory.CreatePeerConnection(observer, new PeerConnectionConfig
                {
                    IceServers = {
                        new PeerConnectionConfig.IceServerInfo {
                            Urls = { "stun:stun.l.google.com:19302" }
                        }
                    }
                });

                _peerConnection.LocalSdpReadytoSend += (string type, string sdp) =>
                {
                    _room.DispatchQueue.ExecuteAsync(async delegate
                    {
                        try
                        {
                            await _room.Peers.First().Signaller.SendAsync("SetAnswer", new
                            {
                                type,
                                sdp
                            });
                        }
                        catch(Exception ex)
                        {
                            _logger.Fatal(ex);
                            throw new NotImplementedException();
                        }
                    });
                };

                // TODO: dispose this
                var remoteDescriptionSetWaitHandler = new ManualResetEvent(false);

                _room.PeerAdded += (object sender, EventArgs<Peer> peerEventArgs) =>
                {
                    peerEventArgs.Payload.RemoteIceCandidateReceived += async (object sender, EventArgs<IceCandidate> e) =>
                    {
                        // Wait for remote description to set first
                        while(true)
                        {
                            if(remoteDescriptionSetWaitHandler.WaitOne(1))
                            {
                                break;
                            }
                            _logger.Warn($"Waiting for remote description to be set..");
                            await Task.Delay(TimeSpan.FromSeconds(1));
                        }

                        await _room.DispatchQueue.ExecuteAsync(delegate
                        {
                            try
                            {
                                _peerConnection.AddIceCandidate(e.Payload.SdpMid, e.Payload.SdpMLineIndex, e.Payload.Candidate);
                                _logger.Info($"Added ice candidate {e.Payload.Candidate}");
                            }
                            catch(Exception ex)
                            {
                                _logger.Fatal(ex);
                                throw new NotImplementedException("TODO: Handle fatal error - need to remove peer from the room");
                            }
                        });
                    };
                    peerEventArgs.Payload.RemoteRtcSessionDescriptionUpdated += (object sender2, EventArgs<RtcSessionDescription> e2) =>
                    {
                        _room.DispatchQueue.ExecuteAsync(async delegate
                        {
                            try
                            {
                                await _peerConnection.SetRemoteDescriptionAsync(e2.Payload.Type, e2.Payload.Sdp);
                                _logger.Info($"Remote description set from peer {peerEventArgs.Payload}");
                                remoteDescriptionSetWaitHandler.Set();

                                // Before creating answer,
                                // prepare local tracks
                                _passiveVideoTrackSource = new PassiveVideoTrackSource();
                                _passiveVideoTrack = new PassiveVideoTrack(Guid.NewGuid().ToString(), _passiveVideoTrackSource);
                                _peerConnection.SetBitrate(1024 * 1024, 1024 * 1024);
                                _peerConnection.AddPassiveVideoTrack(_passiveVideoTrack);

                                // Create answer
                                if(false == _peerConnection.CreateAnswer())
                                {
                                    throw new ApplicationException($"Failed creating answer");
                                }
                            }
                            catch(Exception ex)
                            {
                                _logger.Fatal(ex);
                                throw new NotImplementedException("TODO: Handle fatal error - need to remove peer from the room");
                            }
                        });
                    };
                };

                _peerConnection.RemoteVideoFrameReady += (IntPtr frame) =>
                {
                    _passiveVideoTrackSource.PushVideoFrame(frame);
                };

                // Initialize the connection with a STUN server to allow remote access
                _logger.Debug($"Starting PeerConnection..");
                var config = new PeerConnectionConfiguration
                {
                    IceServers = new List<IceServer> {
                        new IceServer{ Urls = { "stun:stun.l.google.com:19302" } }
                    },
                    SdpSemantic = SdpSemantic.UnifiedPlan
                };

                await _peerConnection.InitializeAsync(config);


                _logger.Info("PeerConnection initalised");
                _room.IsInitialised = true;
            }
            catch(Exception ex)
            {
                _logger.Error(ex);
                throw;
            }
        }

        async Task WaitForPeers()
        {
            while(true)
            {
                var peer = _room.Peers.FirstOrDefault();
                if(null == peer)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    continue;
                }
                break;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
