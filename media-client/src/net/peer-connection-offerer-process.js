import WebSocketMessageHandler from './websocket-message-handler';
import PeerConnectionId from './peer-connection-id';
import FatalErrorHandler from '../handlers/fatal-error-handler';
import Logger from '../logging/logger';

var logger = new Logger('PeerConnectionOfferProcess');

export class PeerConnectionOffererProcess extends WebSocketMessageHandler {
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

    constructor(peerConnection, peerConnectionId, webSocketClient) {
        super(webSocketClient, logger);
        this._peerConnection = peerConnection;
        this._peerConnectionId = peerConnectionId;
    }

    async startAsync() {
        if (this._cancelled) {
            throw 'AlreadyCancelled';
        }

        // Begin the negotiation process by telling the server our transceiver mids
        // (Basically what we will send, for each transceiver)
        try {
            await this._setTransceiversAsync();
        } catch (err) {
            FatalErrorHandler.err(`Failed sending track info ${err}`);
        }

        if (this._cancelled) {
            return;
        }

        // Then Offer first
        var offer = await this._peerConnection.createOffer();
        if (this._cancelled) {
            return;
        }
        // Send the generated sdp to the server
        // Notes - must do this before setLocalDescription()
        // because setLocalDescription() generates ICE candidates,
        // and we don't want to send ICE candidates before the SDP
        this.webSocketClient.queueMessage('SetOffer', {
            offer: {
                type: offer.type,
                sdp: offer.sdp
            },
            peerConnectionId: this._peerConnectionId.hasValue ? this._peerConnectionId.value : null
        });
        logger.info('Offer generated and sent', offer);

        await this._peerConnection.setLocalDescription(offer);
        logger.info('Local description set', offer);
    }

    cancel() {
        if (this._cancelled) {
            return;
        }
        this.stopListeningToWebSocketEvents();
        this._cancelled = true;
    }

    _onAnswer(args) {
        if (!this._peerConnectionId.hasValue) {
            this._peerConnectionId.value = args.peerConnectionId;
        }
        else if (this._peerConnectionId.value != args.peerConnectionId) {
            return;
        }
        if (this._cancelled)
            return;
            
        this._peerConnection.setRemoteDescription(sdp);
        logger.info('Remote SDP received and set');

        // offer process completed,
        // automatically unsubscribe from WebSocket events
        this.stopListeningToWebSocketEvents();
    }

    _setTransceiversAsync() {
        // Timeout - should be longer than the WebSocjet heartbeat timeout,
        // so that if there is connection issue,
        // the WebSocket restarts, better than fail fast.
        const timeout = 30 * 1000;

        return new Promise((resolve, reject) => {
            this._pendingSendTrackInfoResolve = resolve;

            // Tell the servers about the transceivers we have
            var setTransceiverArgs = this._peerConnection.getTransceivers()
                .filter(t => !!t.sender.track)
                .map(function (t) {
                    return {
                        transceiverMid: t.mid,
                        quality: 'High', // TODO: multiple track qualities
                        kind: t.sender.track.kind
                    };
                });

            this.webSocketClient.queueMessage('SetTransceivers', setTransceiverArgs);

            // When timeout, reject the promise.
            window.setTimeout(() => {
                if (this._cancelled) {
                    return;
                }
                if (this._pendingSendTrackInfoResolve != null) {
                    this._pendingSendTrackInfoResolve = null;
                    reject('Timeout');
                }
            }, timeout);
        });
    }

    _onTrackInfoSet() {
        if (this._pendingSendTrackInfoResolve) {
            this._pendingSendTrackInfoResolve();
            this._pendingSendTrackInfoResolve = null;
        }
    }
}
