using MediaServer.Core.Models;
using MediaServer.Models;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.PeerConnection
{
    interface IRenegotationHandler
    {
        /// <summary>
        /// Should be called to start the re-negotation, will be executed on negotation thread.
        /// </summary>
        /// <param name="remoteDevice"></param>
        /// <param name="peerConnection"></param>
        /// <remarks>Can be called from any thread.</remarks>
        /// <returns></returns>
        Task HandleAsync(IRemoteDevice remoteDevice, IPeerConnection peerConnection);
    }
}
