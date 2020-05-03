import PeerConnectionBase from "./peer-connection-base";

export default class PeerConnectionReceiver extends PeerConnectionBase {
    constructor(websocketClient, sender) {
        super(websocketClient, new Logger('PeerConnectionSender'));
        this._handleSenderReady = this._handleSenderReady.bind(this);
        sender.addEventListener('ready', this._handleSenderReady);
    }

    _handleSenderReady() {
        this.restart();
    }
}