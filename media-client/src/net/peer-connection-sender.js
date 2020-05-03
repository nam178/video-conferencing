import WebSocketClient from './websocket-client.js';
import Logger from '../logging/logger.js'
import Queue from '../utils/queue.js';
import PeerConnectionBase from './peer-connection-base.js';
import FatalErrorHandler from '../handlers/fatal-error-handler.js';

export default class PeerConnectionSender extends PeerConnectionBase {
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
        super(websocketClient, new Logger('PeerConnectionSender'));
        this._changeStreamAsync = this._changeStreamAsync.bind(this);
        this._handleRoomJoined = this._handleRoomJoined.bind(this);
        this._changeStreamQueue = new Queue('change-stream', this._changeStreamAsync);
        this.webSocketClient.addEventListener('room', this._handleRoomJoined);
    }

    _handleRoomJoined() {
        // Everytime we join a room, restart the PeerConnection
        // This is due to the life time of a PeerConnection 
        // always associated with the life time of the WebSocket connection
        this.restart();
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
                try
                {
                    await this._sendTrackInfoAsync(tracks[i].id, { quality: 'High', kind: tracks[i].kind });
                }
                catch(err) {
                    this.logger.error('Fatal error: ' + err);
                    FatalErrorHandler.handle(err);
                    return;
                }
            }
            var senderCount = newStream.getTracks().length;
            newStream.getTracks().forEach(track => {
                var rtpSender = this._peerConnection.addTrack(track);
                this._logger.debug(`Added new RtpSender with trackId=${rtpSender.track.id}`);
            });
            this._logger.debug(`Added ${senderCount} new RTP sender(s)`);
        }
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

    _onTrackInfoSet() {
        if (this._pendingSendTrackInfoResolve) {
            this._pendingSendTrackInfoResolve();
        }
    }
}