import PeerConnectionBase from "./peer-connection-base";
import Logger from "../logging/logger"
import FatalErrorHandler from '../handlers/fatal-error-handler.js'
import StreamIndex from '../models/stream-index';
import WebSocketClient from "./websocket-client";
import PeerConnectionSender from "./peer-connection-sender";

export default class PeerConnectionReceiver extends PeerConnectionBase {

    /**
     * @var {StreamIndex}
     */
    _streamIndex;

    /**
     * @param {WebSocketClient} websocketClient 
     * @param {PeerConnectionSender} sender 
     * @param {StreamIndex} streamIndex 
     */
    constructor(websocketClient, sender, streamIndex) {
        super(websocketClient, new Logger('PeerConnectionReceiver'));
        this._senderNegotiationCompleted = this._senderNegotiationCompleted.bind(this);
        this._senderNegotiationStarted = this._senderNegotiationStarted.bind(this);
        this._streamIndex = streamIndex;

        sender.addEventListener('negotiation-started', this._senderNegotiationStarted);
        sender.addEventListener('negotiation-completed', this._senderNegotiationCompleted);
    }

    _senderNegotiationStarted() {
        // The receiver does not listen to web socket events
        // when the sender is negotiating
        // (due to race)
        this.stopListeningToWebSocketEvents();
    }

    _senderNegotiationCompleted() {
        // The receiver does not listen to web socket events
        // when the sender is negotiating
        // (due to race)
        this.startListeningToWebSocketEvents();
        // Restart the id, this is an indication to the server that we are starting new PeerConnection,
        // instead of updating SDP for an existing one.
        this.id = null;
        // The life time of the receiver PeerConnection follows the sender one
        this.restart();
        // The receiver has no track, so sendOffer() won't get called automatically.
        this.sendOfferAsync();
    }

    _onRemoteStream(e) {
        var track = e.track;
        if(!e.streams || e.streams.length == 0) {
           FatalErrorHandler.handle(`Track ${track.id} has no stream`);
        }

        var stream = e.streams[0];
        this._streamIndex.put(stream);

        stream.addEventListener('removetrack', () => {
            this._streamIndex.remove(stream.id);
            this.logger.warn(`track ${track.kind} ${track.id} removed`);
        });
    }
}