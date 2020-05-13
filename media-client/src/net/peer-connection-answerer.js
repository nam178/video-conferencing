import PeerConnectionId from './peer-connection-id'
import Logger from '../logging/logger';

var logger = new Logger('PeerConnectionAnswerer');

export default class PeerConnectionAnswerer {

    /** @type {PeerConnectionId} */
    _peerConnectionId = null;

    /** @type {RTCPeerConnection} */
    _peerConnection = null;

    /**
     * @param {PeerConnectionId} peerConnectionId 
     * @param {RTCPeerConnection} peerConnection
     */
    constructor(peerConnectionId, peerConnection) {
        this._peerConnectionId = peerConnectionId;
        this._peerConnectionId = peerConnection;
    }

    async startAsync(offer) {
        // If we receive remote offer, that means
        // the initial negotiation process (PeerConnectionOfferer) is completed,
        // and we must have a PeerConnection id at this point.
        if (!this._peerConnectionId.hasValue) {
            FatalErrorHandler.failFast('PeerConnectionId was not set at the time received remote offer');
        }

        // This offer is not for me? Ignore it.
        // (There could be more than 1 PeerConnection per client)
        if (offer.peerConnectionId != this._peerConnectionId.value) {
            return;
        }

        var answer = await this._peerConnection.createAnswer();
        logger.debug('[Step 1/3] Answer created', answer);

        await this._peerConnection.setLocalDescription(answer);
        logger.debug('[Step 2/3] Answer set as local description', answer);

        this.webSocketClient.queueMessage('SetAnswer', {
            answer: {  
                type: answer.type,
                sdp: answer.sdp
            },
            peerConnectionId: this.peerConnection.value
        });
        logger.info('[Step 3/3] Answer sent');
    }
}