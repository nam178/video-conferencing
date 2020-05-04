import PeerConnectionBase from "./peer-connection-base";
import Logger from "../logging/logger"

export default class PeerConnectionReceiver extends PeerConnectionBase {
    constructor(websocketClient, sender) {
        super(websocketClient, new Logger('PeerConnectionReceiver'));
        this._senderNegotiationCompleted = this._senderNegotiationCompleted.bind(this);
        this._senderNegotiationStarted = this._senderNegotiationStarted.bind(this);

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
}