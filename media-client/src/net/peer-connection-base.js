import Logger from '../logging/logger.js';

export default class PeerConnectionBase {
    /**
     * @return {WebSocketClient}
     */
    get webSocketClient() { return this._webSocketClient; }

    /**
     * @returns {Logger}
     */
    get logger() { return this._logger; }

    /**
     * @var {Logger}
     */
    _logger = null;

    constructor(websocketClient, logger) {
        if (!websocketClient) {
            throw 'Must specify webSocketClient';
        }
        this._handleWebSocketMessage = this._handleWebSocketMessage.bind(this);
        this._webSocketClient = websocketClient;
        this._webSocketClient.addEventListener('message', this._handleWebSocketMessage);
        this._logger = logger;
    }

    /**
     * WebSocket connection must be ready first
     */
    restart() {
        if (this._peerConnection) {
            this._peerConnection.close();
        }
        this._peerConnection = new RTCPeerConnection({
            sdpSemantics: 'unified-plan',
            iceServers: [
                { urls: STUN_URLS }
            ]
        });
        this._peerConnection.addEventListener('icecandidate', e => {
            this._logger.debug('Generated local ICE candidate', e);
            if (e.candidate)
                this._sendIceCandidate(e.candidate);
        });
        this._peerConnection.addEventListener('iceconnectionstatechange', e => {
            this._logger.debug('ice state change', e);
            if (this._peerConnection.iceConnectionState === "failed" ||
                this._peerConnection.iceConnectionState === "disconnected" ||
                this._peerConnection.iceConnectionState === "closed") {
                // TODO Handle ice connection failure
            }
        });
        this._peerConnection.addEventListener("negotiationneeded", () => {
            this._logger.warn('re-negotiation needed');
            this._sendOfferAsync();
        }, false);
        this._peerConnection.addEventListener('track', e => {
            this._logger.info('Received remote stream', e);
            // TODO: update tracks
            // document.getElementById('remote_video').srcObject = e.streams[0];
        });
        this._logger.log('PeerConnection initialised');
        this._changeStream(null, this._localMediaStreamForSending);
    }

    _handleWebSocketMessage(e) {
        var commandName = `_on${e.detail.command}`;
        if (typeof this[commandName] != 'undefined') {
            e.preventDefault();
            this._logger.debug(commandName, e.detail.args);
            this[commandName](e.detail.args);
        }
    }

    async _sendOfferAsync() {
        // Generate a token to prevent race condition when this method
        // is called multiple times and the async continuation below
        // race with each other:
        var token = {};
        this._token = token;
        var offer = await this._peerConnection.createOffer({
            offerToReceiveAudio: 1,
            offerToReceiveVideo: 1
        });
        this._logger.info('Generated offer', offer);
        await this._peerConnection.setLocalDescription(offer);
        // If there is newer token,
        // cancel this;
        if (this._token != token) {
            return;
        }
        // Send the generated sdp to the server
        this.webSocketClient.queueMessageForSending('SetOffer', {
            offer: {
                type: offer.type,
                sdp: offer.sdp
            }
        });
    }

    _sendIceCandidate(candidate) {
        this._logger.log('Sending ice candidate', candidate);
        this.webSocketClient.queueMessageForSending('AddIceCandidate', {
            candidate: {
                candidate: candidate.candidate,
                sdpMid: candidate.sdpMid,
                sdpMLineIndex: candidate.sdpMLineIndex
            }
        });
    }

    _onIceCandidate(iceCandidate) {
        if (!this._peerConnection) {
            this._logger.warn('Received ICE candidate without PeerConnection');
            return;
        }
        this._peerConnection.addIceCandidate(iceCandidate);
        this._logger.debug('Remote ICE candidate received', iceCandidate);
    }

    _onAnswer(sdp) {
        this._setSdp(sdp);
    }

    _setSdp(sdp) {
        if (!this._peerConnection) {
            this._logger.warn('Received answer/offer without PeerConnection');
            return;
        }
        this._peerConnection.setRemoteDescription(sdp);
        this._logger.info('Remote SDP received');
    }
}