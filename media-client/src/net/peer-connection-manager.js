import WebSocketClient from './websocket-client.js';
import Logger from '../logging/logger.js'

export default class PeerConnectionManager {
    /**
     * @return {WebSocketClient}
     */
    get webSocketClient() { return this._webSocketClient; }

    /**
     * @return {MediaStream}
     */
    get localMediaStreamForSending() { return this._localMediaStreamForSending; }

    /**
     * @param {MediaStream} value
     */
    set localMediaStreamForSending(value) 
    {
        if(!this._peerConnection) {
            throw 'PeerConnectionNotInitialized';
        }

        if(value != this._localMediaStreamForSending)
        {
            // Remove previous tracks if exist
            if(this.localMediaStreamForSending) {
                this.localMediaStreamForSending
                    .getTracks()
                    .forEach(track => {
                        this._peerConnection.removeTrack(track);
                        this._logger.debug(`Removed local track`, track);
                    });
            }

            this._localMediaStreamForSending = value;
            if(value) {
                value.getTracks().forEach(track => {
                    this._peerConnection.addTrack(track);
                    this._logger.info('Added local track', track);
                });
            }
        }
    }

    /**
     * @param {WebSocketClient} websocketClient 
     */
    constructor(websocketClient) {
        if (!websocketClient) {
            throw 'Must specify webSocketClient';
        }
        this._webSocketClient = websocketClient;
        this._webSocketClient.addEventListener('message', this._handleWebSocketMessage);
        this._logger = new Logger('PeerConnectionManager');
        this._handleWebSocketMessage = this._handleWebSocketMessage.bind(this);
    }

    /**
     * WebSocket connection must be ready first
     */
    initialize() {
        this._peerConnection = new RTCPeerConnection({
            sdpSemantics: 'unified-plan',
            iceServers: [
                { urls: STUN_URLS }
            ]
        });
        this._peerConnection.addEventListener('icecandidate', e => {
            this._logger.debug('Received local ICE candidate', e);
            if (e.candidate)
                this._sendIceCandidate(e.candidate);
        });
        this._peerConnection.addEventListener('iceconnectionstatechange', e => {
            this._logger.debug('ice state change', e);
            if (pc.iceConnectionState === "failed" ||
                pc.iceConnectionState === "disconnected" ||
                pc.iceConnectionState === "closed") {
                // TODO Handle ice connection failure
            }
        });
        this._peerConnection.addEventListener("negotiationneeded", () => {
            this._logger.warn('re-negotiation needed');
            this._sendOffer();
        }, false);
        this._peerConnection.addEventListener('track', e => {
            this._logger.info('Received remote stream', e);
            // TODO: update tracks
            // document.getElementById('remote_video').srcObject = e.streams[0];
        });
        this._logger.log('PeerConnection initialised');
    }

    async _sendOffer() {
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

    _onSetIceCandidate(args) {
        // TODO
    }

    _onSetAnswer(args) {
        // TODO
    }

    _handleWebSocketMessage(e) {
        var commandName = `_on${e.detail.command}`;
        if (typeof this[commandName] != 'undefined') {
            e.preventDefault();
            this._logger.debug(commandName, e.detail.args);
            this[commandName](e.detail.args);
        }
    }
}