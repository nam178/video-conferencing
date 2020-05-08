import Logger from '../logging/logger.js';

export default class PeerConnectionBase extends EventTarget {
    /**
     * @return {WebSocketClient}
     */
    get webSocketClient() { return this._webSocketClient; }

    /**
     * @returns {Logger}
     */
    get logger() { return this._logger; }

    /**
     * Id of this PeerConnection, set by the server, will return in setAnswer()
     * @returns {String}
     */
    get id() { return this._id; }

    /**
     * @param {String} value
     */
    set id(value) { this._id = value; }

    /**
     * @var {Logger}
     */
    _logger = null;

    /**
     * @var {RTCIceCandidate[]}
     */
    _pendingIceCandidates = [];

    constructor(websocketClient, logger) {
        if (!websocketClient) {
            throw 'Must specify webSocketClient';
        }
        super();
        this._handleWebSocketMessage = this._handleWebSocketMessage.bind(this);
        this._webSocketClient = websocketClient;
        this._logger = logger;
    }

    startListeningToWebSocketEvents() {
        this._webSocketClient.addEventListener('message', this._handleWebSocketMessage);
    }

    stopListeningToWebSocketEvents() {
        this._webSocketClient.removeEventListener('message', this._handleWebSocketMessage);
    }

    /**
     * WebSocket connection must be ready first
     */
    restart() {
        this.logger.warn('Restarting..');
        this.id = null;
        if (this._peerConnection) {
            this._peerConnection.close();
        }
        this._pendingIceCandidates = [];
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
            this.sendOfferAsync();
        }, false);
        this._peerConnection.addEventListener('track', e => {
            this._logger.info('Received remote stream', e);
            if(this._onRemoteStream) {
                this._onRemoteStream(e);
            }
        });
        this._logger.log('PeerConnection initialised');
    }

    async sendOfferAsync() {
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
            },
            peerConnectionId: this.id
        });
    }

    _sendIceCandidate(candidate) {
        // If id was not set, that means offer still being processed,
        // we will wait a bit and try again later
        if (this.id == null) {
            this._pendingIceCandidates.push(candidate);
            return;
        }
        this._logger.log('Sending ice candidate', candidate);
        this.webSocketClient.queueMessageForSending('AddIceCandidate', {
            candidate: {
                candidate: candidate.candidate,
                sdpMid: candidate.sdpMid,
                sdpMLineIndex: candidate.sdpMLineIndex
            },
            peerConnectionId: this.id
        });
    }

    _onAnswer(args) {
        if (this.id == null) {
            this.id = args.peerConnectionId;
        } else if (this.id != args.peerConnectionId) {
            return;
        }
        this._setSdp(args.sdp);
        this.dispatchEvent(new CustomEvent('negotiation-completed'));

        // Flush pending ice candidates
        this._pendingIceCandidates.forEach(ice => this._sendIceCandidate(ice));
        this.logger.info(`Flushed ${this._pendingIceCandidates.length} pending ICE candidates`);
        this._pendingIceCandidates = [];
    }

    async _onOffer(args) {
        if (this.id != args.peerConnectionId) {
            return;
        }
        this._setSdp(args.sdp);
         // Generate a token to prevent race condition when this method
        // is called multiple times and the async continuation below
        // race with each other:
        var token2 = {};
        this._token2 = token2;
        var answer = await this._peerConnection.createAnswer();
        this._logger.info('Generated answer', answer);
        await this._peerConnection.setLocalDescription(answer);
        // If there is newer token,
        // cancel this;
        if (this._token2 != token2) {
            return;
        }
        // Send the generated sdp to the server
        this.webSocketClient.queueMessageForSending('SetAnswer', {
            answer: {
                type: answer.type,
                sdp: answer.sdp
            },
            peerConnectionId: this.id
        });
    }

    _onIceCandidate(args) {
        if (this.id != args.peerConnectionId) {
            return;
        }
        if (!this._peerConnection) {
            this._logger.warn('Received ICE candidate without PeerConnection');
            return;
        }
        this._peerConnection.addIceCandidate(args.candidate);
        this._logger.debug('Remote ICE candidate received', args);
    }

    _setSdp(sdp) {
        if (!this._peerConnection) {
            this._logger.warn('Received answer/offer without PeerConnection');
            return;
        }
        this._peerConnection.setRemoteDescription(sdp);
        this._logger.info('Remote SDP received and set');
    }

    _handleWebSocketMessage(e) {
        var commandName = `_on${e.detail.command}`;
        if (typeof this[commandName] != 'undefined') {
            this._logger.debug(`Executing method ${commandName} with args=`, e.detail.args);
            this[commandName](e.detail.args);
        }
    }
}