import WebSocketMessageHandler from './websocket-message-handler';
import PeerConnectionId from './peer-connection-id';
import FatalErrorHandler from '../handlers/fatal-error-handler';
import Logger from '../logging/logger';
import sleep from '../utils/sleep'

var logger = new Logger('PeerConnectionOfferProcess');

/**
 * @param {WebSocketClient} webSocketClient
 * @param {RTCIceCandidate} candidate
 * @param {PeerConnectionId} peerConnectionId
 */
function sendIceCandidate(webSocketClient, candidate, peerConnectionId) {
    if (!peerConnectionId.hasValue) {
        debugger;
    }
    webSocketClient.queueMessage('AddIceCandidate', {
        candidate: {
            candidate: candidate.candidate,
            sdpMid: candidate.sdpMid,
            sdpMLineIndex: candidate.sdpMLineIndex
        },
        peerConnectionId: peerConnectionId.value
    });
    logger.log('Local ICE candidate sent', candidate);
}

/**
 * @param {RTCPeerConnection} peerConnection 
 * @returns {Object[]}
 */
function getTransceiverMetadata(peerConnection) {
    // Tell the servers about the transceivers we have
    return peerConnection.getTransceivers()
        .filter(t => !!t.sender.track)
        .map(function (t) {
            return {
                transceiverMid: t.mid,
                quality: 'High', // TODO: multiple track qualities
                kind: t.sender.track.kind
            };
        });
}

export default class PeerConnectionOfferer extends WebSocketMessageHandler {
    /**
     * @type {RTCPeerConnection}
     */
    _peerConnection = null;

    /**
     * @type {Boolean}
     */
    _cancelled = false;

    /**
     * @type {PeerConnectionId}
     */
    _peerConnectionId = null;

    /** @type {RTCIceCandidate[]} */
    _pendingIceCandidates = [];

    constructor(peerConnection, peerConnectionId, webSocketClient) {
        super(webSocketClient, logger);
        this._peerConnection = peerConnection;
        this._peerConnectionId = peerConnectionId;
        this._onLocalIceCandidate = this._onLocalIceCandidate.bind(this);
        this._onLocalIceConnectionChange = this._onLocalIceConnectionChange.bind(this);
    }

    async startAsync() {
        if (this._cancelled) throw 'AlreadyCancelled';
        if (this._started) throw 'AlreadyStarted';
        this._started = true;
        this.startObservingWebSocketMessages();

        // Preparation: listen to ICE candidate events
        this._peerConnection.addEventListener('icecandidate', this._onLocalIceCandidate);
        this._peerConnection.addEventListener('iceconnectionstatechange', this._onLocalIceConnectionChange);

        // Step 1: Create Offer
        var offer = await this._peerConnection.createOffer();
        if (this._cancelled) {
            return;
        }
        logger.info('[Step 1/5] Offer generated', offer);

        // Step 2: Set Local Description.
        // This generates ICE candidates, however they will be queued,
        // as sending ICE candidates must be done after SDP is set.
        await this._peerConnection.setLocalDescription(offer);
        if (this._cancelled)
            return;
        logger.info('[Step 2/5] Local description set', offer);

        // Send the generated sdp to the server.
        // Must do this after setLocalDescription, because the transceiver mids are null
        // until we call setLocalDescription();
        if (false == this.webSocketClient.tryQueueMessage('SetOffer', {
            offer: {
                type: offer.type,
                sdp: offer.sdp
            },
            peerConnectionId: this._peerConnectionId.hasValue ? this._peerConnectionId.value : null,
            transceiverMetadata: getTransceiverMetadata(this._peerConnection)
        })) {
            // Again, for WebSocket errors, best way to handle this
            // is to cancel the entire offer process. (See comment above)
            this.cancel();
            return;
        }
        logger.info('[Step 3/5] Offer sent', offer);
    }

    cancel() {
        if (this._cancelled) {
            return;
        }
        this.stopObservingWebSocketMessages();
        this._peerConnection.removeEventListener('icecandidate', this._onLocalIceCandidate);
        this._peerConnection.removeEventListener('iceconnectionstatechange', this._onLocalIceConnectionChange);
        this._cancelled = true;
    }

    // called when 'icecandidate' event occurs on local PeerConnection
    async _onLocalIceCandidate(e) {
        if (e.candidate) {
            // We shouldn't be sending ICE candidates unless the offer has been sent,
            // As ICE candidate can't be added (on the remote side) unless SDP is set.
            if (this._pendingIceCandidates)
                this._pendingIceCandidates.push(e.candidate);
            else
                sendIceCandidate(this.webSocketClient, e.candidate, this._peerConnectionId);
        }
    }

    // called when 'iceconnectionstatechange' event occurs on local PeerConnection
    _onLocalIceConnectionChange(e) {
        // logger.debug('ice state change', e);
        if (this._peerConnection.iceConnectionState === "failed" ||
            this._peerConnection.iceConnectionState === "disconnected" ||
            this._peerConnection.iceConnectionState === "closed") {
            // TODO Handle ice connection failure
            logger.error('TODO: handle ICE connection failure');
        }
    }

    // called when we receive 'Answer' message from websocket
    async _onAnswer(args) {
        if (!this._peerConnectionId.hasValue) {
            this._peerConnectionId.value = args.peerConnectionId;
        }
        else if (this._peerConnectionId.value != args.peerConnectionId) {
            return;
        }
        if (this._cancelled) return;

        try {
            await this._peerConnection.setRemoteDescription(args.sdp);
        }
        catch (err) {
            FatalErrorHandler.failFast(`Failed setting remote description`);
        }

        if (this._cancelled) return;
        logger.info(`[Step 4/5] Received answer and remote SDP set`);

        // Final step is to flush any pending ICE candidates
        this._pendingIceCandidates
            .forEach(candidate => sendIceCandidate(
                this.webSocketClient, 
                candidate, 
                this._peerConnectionId));
        logger.info(`[Step 5/5] Flushed ${this._pendingIceCandidates.length} ICE candidates`)
        this._pendingIceCandidates = [];

        // offer process completed,
        // automatically unsubscribe from WebSocket events
        this.stopObservingWebSocketMessages();
    }

    _onTransceiversMetadataSet() {
        if (this._pendingSendTrackInfoResolve) {
            this._pendingSendTrackInfoResolve();
            this._pendingSendTrackInfoResolve = null;
        }
    }
}
