import PeerConnectionId from './peer-connection-id'
import Logger from '../logging/logger';
import PeerConnectionMediaHandler from './peer-connection-media-handler';
import FatalErrorHandler from '../handlers/fatal-error-handler';
import WebSocketClient from './websocket-client';

var logger = new Logger('PeerConnectionAnswerer');

export default class PeerConnectionAnswerer {

    /** @type {PeerConnectionId} */
    _peerConnectionId = null;

    /** @type {RTCPeerConnection} */
    _peerConnection = null;

    /** @type {PeerConnectionMediaHandler} */
    _mediaHandler = null;

    /** @type {Boolean} */
    _isCanceled = false;

    /**
     * @param {PeerConnectionId} peerConnectionId 
     * @param {RTCPeerConnection} peerConnection
     * @param {PeerConnectionMediaHandler} mediaHandler
     * @param {WebSocketClient} webSocketClient
     */
    constructor(peerConnectionId, peerConnection, mediaHandler, webSocketClient) {
        this._peerConnectionId = peerConnectionId;
        this._peerConnection = peerConnection;
        this._mediaHandler = mediaHandler;
        this._webSocketClient = webSocketClient;
    }

    cancel() {
        this._isCanceled = true;
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

        logger.info('[Step 0/4] Remote offer received, answering..', answer);

        this._mediaHandler.setTransceiversMetadata(offer.transceivers);
        try {
            await this._peerConnection.setRemoteDescription(offer.sdp);
        }
        catch (err) { // likely that setRemoteDescription will fail due to sdp format issue
            FatalErrorHandler.failFast(`setRemoteDescription failed ${err}`);
        }
        if (this._isCanceled) {
            return;
        }

        logger.info('[Step 1/4] Remote offer received and set', answer);

        var answer = await this._peerConnection.createAnswer();
        if (this._isCanceled) {
            return;
        }
        logger.info('[Step 2/4] Answer created', answer);

        await this._peerConnection.setLocalDescription(answer);
        if (this._isCanceled) {
            return;
        }
        logger.info('[Step 3/4] Answer set as local description', answer);

        this._webSocketClient.queueMessage('SetAnswer', {
            offerId: offer.offerId,
            answer: {
                type: answer.type,
                sdp: answer.sdp
            },
            peerConnectionId: this._peerConnectionId.value
        });
        logger.info('[Step 4/4] Answer sent');
    }
}