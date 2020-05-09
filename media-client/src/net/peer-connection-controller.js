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
 * @param {RTCPeerConnection} peerConnection 
 * @param {MediaStreamTrack} prevTrack 
 * @param {MediaStreamTrack} nextTrack 
 */
function replaceTrack(peerConnection, prevTrack, nextTrack) {
    if (prevTrack) {
        var prevAudioTransceiver = peerConnection
            .getTransceivers()
            .filter(trans => trans.sender.track == prevTrack)
            .shift();
        if (!prevAudioTransceiver) {
            FatalErrorHandler.failFast(`Failed finding transceiver for track ${prevTrack.id} ${prevTrack.kind}`);
        }
        prevAudioTransceiver.sender.replaceTrack(nextTrack);
    }
    else {
        peerConnection.addTrack(nextTrack);
    }
}

/**
 * Should be called everytime the PeerConnection restarts,
 * or local media stream change, so we can re-associate them.
 * 
 * @param {MediaStream} oldStream 
 * @param {MediaStream} newStream 
 */
function setStream(peerConnection, oldStream, newStream) {
    if (peerConnection == null) {
        throw 'SetStreamCalledWhilePeerConnectionIsNull';
    }
    // PeerConnection exists but its stream has not changed?
    // Early exit.
    if (oldStream == newStream) {
        logger.debug('Stream not changed, ignoring the change.');
        return;
    }
    logger.info('Local media stream changed, updating PeerConnection..');

    // If there is existing stream, we will REPLACE transceiver's tracks
    var prevAudioTrack = getFirstTrackOrNull(oldStream, 'audio');
    var prevVideoTrack = getFirstTrackOrNull(oldStream, 'video');
    var nextAudioTrack = getFirstTrackOrNull(newStream, 'audio');
    var nextVideoTrack = getFirstTrackOrNull(newStream, 'video');

    replaceTrack(peerConnection, prevAudioTrack, nextAudioTrack);
    replaceTrack(peerConnection, prevVideoTrack, nextVideoTrack);

    logger.debug('New local audio track has been set', nextAudioTrack);
    logger.debug('New local video track has been set', nextVideoTrack);
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

    /**
     * @return {MediaStream}
     */
    get localStream() { return this._localStream; }

    /**
     * @param {MediaStream} value
     */
    set localStream(value) {
        var oldStream = this._localStream;
        this._localStream = value;
        if(this._peerConnection)
        {
            if(this._currentOfferer && !this._currentOfferer.isCompleted) {
                // There's some negotiation underway
                // Wait for negotiation complete before changing the stream again,
                // because changing the stream causes re-negotiation
                this._currentOfferer.onComplete(() => setStream(this._peerConnection, oldStream, value));
            } else {
                setStream(this._peerConnection, oldStream, value);    
            }
        }
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
        // always associated with the life time of the WebSocket connection
        this._restart();

        // Set the track if we have at least one track,
        // this will then trigger "negotiation required", therefore starts the offer process.
        if (this._localStream && this._localStream.getTracks().length > 0) {
            setStream(this._peerConnection, null, this._localStream);
        }
        // Else, if we have no track, manually start the offer process.
        // Otherwise PeerConnection will never established.
        else {
            logger.warn('No local media stream, starting offer process manually..');
            this._startOfferProcess();
        }
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
        logger.info('PeerConnection created');
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