using MediaServer.Core.Models;
using MediaServer.Models;
using NLog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.PeerConnection
{
    sealed class RenegotationHandler : IRenegotationHandler
    {
        readonly ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        public Task HandleAsync(IRemoteDevice remoteDevice, IPeerConnection peerConnection)
        {
            if(peerConnection.Room == null)
                throw new InvalidProgramException();

            // Begin the actual re-negotation
            // Must do this in a queue to avoid race between requests
            _logger.Trace($"Re-negotiating with {peerConnection}..");
            return peerConnection.Room.RenegotiationQueue.ExecuteAsync(async delegate
            {
                try
                {
                    // Order important here: generate offer first.
                    var offer = await peerConnection.CreateOfferAsync();
                    
                    // TODO: send track info here

                    // Then send the SDP before SetLocalSessionDescription, 
                    // so the sdp processed by remote peer before they process ICE candidates,
                    // those generated from SetLocalSessionDescriptionAsync();
                    remoteDevice.EnqueueSessionDescription(peerConnection.Id, offer);

                    // then send candidates later so they processed after the SDP is processed.
                    await peerConnection.SetLocalSessionDescriptionAsync(offer);
                    _logger.Info($"Re-negotiating offer sent to {peerConnection}.");
                }
                catch(Exception ex) when(ex is ObjectDisposedException || ex is TaskCanceledException || ex is InvalidOperationException)
                {
                    _logger.Warn($"Failed re-negotiating due to PeerConnection closed, will terminate remote device. Err={ex.Message}");
                }
                catch(IOException ex)
                {
                    _logger.Warn($"Failed re-negotiating due to IO, will terminate remote device. Err={ex.Message}");
                }
                catch(Exception ex)
                {
                    _logger.Error(ex, "Unexpected error while re-negotiating");
                }
            });
        }

    }
}
