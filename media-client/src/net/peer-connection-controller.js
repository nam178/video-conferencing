import Logger from '../logging/logger.js'
import FatalErrorHandler from '../handlers/fatal-error-handler.js';
import WebSocketMessageHandler from './websocket-message-handler';
import PeerConnectionOfferer from './peer-connection-offerer';
import PeerConnectionId from './peer-connection-id';

var logger = new Logger('PeerConnectionController');

/**
 * @param {MediaStream} stream 
 * @param {String} trackKind 
 */
function getFirstTrackOrNull(stream, trackKind) {
    // always return something or NULL
    if (stream) {
        var tmp = stream.getTracks().filter(track => track.kind == trackKind).shift();
        return tmp ? tmp : null;
    }
    return null;
}

/**
 * Replace transceiver's track
 * 
 * @param {RTCRtpTransceiver} transceiver 
 * @param {MediaStream} stream 
 * @param {String} kind 
 */
function replaceTrack(transceiver, stream, kind) {
    transceiver.sender.replaceTrack(getFirstTrackOrNull(stream, kind));
}

export default class PeerConnectionController extends WebSocketMessageHandler {

    /** @type {RTCPeerConnection} */
    _peerConnection = null;
    /** @type {PeerConnectionId} */
    _peerConnectionId = null;
    /** @type {PeerConnectionOfferer} */
    _currentOfferer = null;
    /** @type {MediaStream} */
    _localStream = null;
    /** @type {RTCRtpTransceiver} */
    _audioTransceiver = null;
    /** @type {RTCRtpTransceiver} */
    _videoTransceiver = null;

    /**
     * @return {MediaStream}
     */
    get localStream() { return this._localStream; }

    /**
     * @param {MediaStream} value
     */
    set localStream(value) {
        this._localStream = value;
        if(this._peerConnection)
            this._replaceTracksFromStream(value);
    }

    constructor(webSocketClient) {
        super(webSocketClient, 'PeerConnectionController');

        // Listen to 'room' event -> _handleRoomJoined
        this._handleRoomJoined = this._handleRoomJoined.bind(this);
        this.webSocketClient.addEventListener('room', this._handleRoomJoined);
    }

    _handleRoomJoined() {
        // Everytime we join a room, restart the PeerConnection
        // This is due to the life time of a PeerConnection 
        // always associated with the life time of the WebSocket connection.
        // 
        // The 'negotiationneeded' event will be triggered (due to
        // transceivers are created) and the negotiation process starts
        // immediately.
        this._restart();
        
        // At this point, PeerConnection is created and the negotation process
        // will start. We set tracks for the transceivers: (this won't cause 
        // re-negotiation)
        this._replaceTracksFromStream(this._localStream);
    }

    _restart() {
        logger.warn('Restarting..');
        this._peerConnectionId = new PeerConnectionId();
        this._cancelAnyRunningOfferProcess();
        if (this._peerConnection) {
            this._peerConnection.close();
        }
        this._peerConnection = new RTCPeerConnection({
            sdpSemantics: 'unified-plan',
            iceServers: [{ urls: STUN_URLS }]
        });
        this._peerConnection.addEventListener("negotiationneeded", () => {
            logger.warn('re-negotiation needed, starting new offerer process..');
            this._startOfferProcess();
        }, false);
        this._peerConnection.addEventListener('track', e => {
            logger.info('Received remote stream', e);
            if (this._onRemoteStream) {
                this._onRemoteStream(e);
            }
        });
        this._audioTransceiver = this._peerConnection.addTransceiver('audio');
        this._videoTransceiver = this._peerConnection.addTransceiver('video');
        logger.info('PeerConnection created');
    }

    _replaceTracksFromStream(stream) {
        replaceTrack(this._audioTransceiver, stream, 'audio');
        replaceTrack(this._videoTransceiver, stream, 'video');
    }

    _startOfferProcess() {
        this._cancelAnyRunningOfferProcess();
        this._currentOfferer = new PeerConnectionOfferer(
            this._peerConnection,
            this._peerConnectionId,
            this.webSocketClient);
        this._currentOfferer
            .startAsync()
            .catch(err => {
                FatalErrorHandler.failFast(`Failed starting the Offerer process: ${err}`);
                this._currentOfferer.cancel();
                this._currentOfferer = null;
            });
    }

    _cancelAnyRunningOfferProcess() {
        if (this._currentOfferer != null) {
            this._currentOfferer.cancel();
            this._currentOfferer = null;
        }
    }
}