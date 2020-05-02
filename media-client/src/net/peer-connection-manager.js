import WebSocketClient from './websocket-client.js';
import Logger from '../logging/logger.js'
import Queue from '../utils/queue.js';

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
    set localMediaStreamForSending(value) {
        var oldStream = this._localMediaStreamForSending;
        this._localMediaStreamForSending = value;
        this._changeStream(oldStream, value);
    }

    /**
     * @var {Queue}
     */
    _changeStreamQueue;

    /**
     * @param {WebSocketClient} websocketClient 
     */
    constructor(websocketClient) {
        if (!websocketClient) {
            throw 'Must specify webSocketClient';
        }
        this._changeStreamAsync = this._changeStreamAsync.bind(this);
        this._changeStreamQueue = new Queue('change-stream', this._changeStreamAsync);
        this._handleWebSocketMessage = this._handleWebSocketMessage.bind(this);
        this._handleRoomJoined = this._handleRoomJoined.bind(this);
        this._webSocketClient = websocketClient;
        this._webSocketClient.addEventListener('message', this._handleWebSocketMessage);
        this._webSocketClient.addEventListener('room', this._handleRoomJoined);
        this._streamWasSetOnPeerConnection = null;
        this._logger = new Logger('PeerConnectionManager');
    }

    /**
     * WebSocket connection must be ready first
     */
    _restart() {
        if (this._peerConnection) {
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

    _handleRoomJoined() {
        // Everytime we join a room, restart the PeerConnection
        // This is due to the life time of a PeerConnection 
        // always associated with the life time of the WebSocket connection
        this._restart();
    }

    _changeStream(oldStream, newStream) {
        this._changeStreamQueue.enqueue([oldStream, newStream]);
    }

    async _changeStreamAsync(queueItem) {
        var oldStream = queueItem[0];
        var newStream = queueItem[1];
        // Should be called everytime the PeerConnection restarts,
        // or local media stream change, so we can re-associate
        // them.
        if (this._peerConnection == null) {
            return;
        }
        // PeerConnection exists but its stream has not changed?
        // Early exit.
        if (oldStream == newStream) {
            this._logger.debug('Stream not changed, ignoring the change.');
            return;
        }
        this._logger.debug('Local media stream changed, updating PeerConnection..');
        // Pass this point strema has changed,
        // we'll get rid of existing stream.
        if (oldStream) {
            var senderCount = this._peerConnection.getSenders().length;
            this._peerConnection.getSenders().forEach(sender => this._peerConnection.removeTrack(sender));
            this._logger.debug(`Removed ${senderCount} old RTP sender(s)`);
        }
        // Then add tracks for the new stream, if it exists
        if (newStream) {
            // Need to tell the server about the tracks that we're sending,
            // otherwise it will reject it.
            var tracks = newStream.getTracks();
            for (var i in tracks) {
                await this._sendTrackInfoAsync(tracks[i].id, { quality: 'High', kind: tracks[i].kind });
            }
            var senderCount = newStream.getTracks().length;
            newStream.getTracks().forEach(track => {
                var rtpSender = this._peerConnection.addTrack(track);
                this._logger.debug(`Added new RtpSender with trackId=${rtpSender.track.id}`);
            });
            this._logger.debug(`Added ${senderCount} new RTP sender(s)`);
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

    _sendTrackInfoAsync(trackId, trackProperties) {
        const timeout = 15 * 1000;

        return new Promise((resolve, reject) => {
            this._pendingSendTrackInfoResolve = resolve;
            this.webSocketClient.queueMessageForSending('SetTrackInfo', {
                ...trackProperties,
                trackId: trackId
            });
            // Timeout:
            window.setTimeout(() => {
                this._pendingSendTrackInfoResolve = null;
                reject('Timeout');
            }, timeout);
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

    _onOffer(sdp) {
        this._setSdp(sdp);
    }

    _onTrackInfoSet() {
        if (this._pendingSendTrackInfoResolve) {
            this._pendingSendTrackInfoResolve();
        }
    }

    _setSdp(sdp) {
        if (!this._peerConnection) {
            this._logger.warn('Received answer/offer without PeerConnection');
            return;
        }
        this._peerConnection.setRemoteDescription(sdp);
        this._logger.info('Remote SDP received');
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