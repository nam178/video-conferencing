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
        this._localMediaStreamForSending = value;
        this._restartLocalMediaStream();
    }

    /**
     * @param {WebSocketClient} websocketClient 
     */
    constructor(websocketClient) {
        if (!websocketClient) {
            throw 'Must specify webSocketClient';
        }
        this._handleWebSocketMessage = this._handleWebSocketMessage.bind(this);
        this._handleRoomJoined = this._handleRoomJoined.bind(this);
        this._webSocketClient = websocketClient;
        this._webSocketClient.addEventListener('message', this._handleWebSocketMessage);
        this._webSocketClient.addEventListener('room',  this._handleRoomJoined);
        this._streamWasSetOnPeerConnection = null;
        this._logger = new Logger('PeerConnectionManager');
    }

    /**
     * WebSocket connection must be ready first
     */
    _restart() {
        if(this._peerConnection) {
            this._peerConnection.close();
            this._streamWasSetOnPeerConnection = null;
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
        this._restartLocalMediaStream();
    }

    _handleRoomJoined() {
        // Everytime we join a room, restart the PeerConnection
        // This is due to the life time of a PeerConnection 
        // always associated with the life time of the WebSocket connection
        this._restart();
    }

    _restartLocalMediaStream() {
        // Should be called everytime the PeerConnection restarts,
        // or local media stream change, so we can re-associate
        // them.
        if(this._peerConnection == null) {
            return;
        }
        // PeerConnection exists but its stream has not changed?
        // Early exit.
        if(this._streamWasSetOnPeerConnection == this._localMediaStreamForSending) 
            {
                this._logger.debug('Local media stream not changed, ignoring the change.');
                return;
            }
        this._logger.debug('Local media stream changed, updating PeerConnection..');
        // Pass this point strema has changed,
        // we'll get rid of existing stream.
        if(this._streamWasSetOnPeerConnection) {
            var senderCount = this._peerConnection.getSenders().length;
            this._peerConnection.getSenders().forEach(sender => this._peerConnection.removeTrack(sender));
            this._logger.debug(`Removed ${senderCount} old RTP sender(s)`);
        }
        // Then add tracks for the new stream, if it exists
        if(this._localMediaStreamForSending) {
            var senderCount  = this._localMediaStreamForSending.getTracks().length;
            this._localMediaStreamForSending.getTracks().forEach(track => this._peerConnection.addTrack(track));
            this._logger.debug(`Added ${senderCount} new RTP sender(s)`);
        }
        // Save the current stream for next time.
        this._streamWasSetOnPeerConnection = this.localMediaStreamForSending;
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

    _onIceCandidate(iceCandidate) {
        if(!this._peerConnection) {
            this._logger.warn('Received ICE candidate without PeerConnection');
            return;
        }
        this._peerConnection.addIceCandidate(iceCandidate);
    }

    _onAnswer(sdp) {
        this._setSdp(sdp);
    }

    _onOffer(sdp) {
        this._setSdp(sdp);
    }

    _setSdp(sdp) {
        if(!this._peerConnection) {
            this._logger.warn('Received answer/offer without PeerConnection');
            return;
        }
        this._peerConnection.setRemoteDescription(sdp);
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