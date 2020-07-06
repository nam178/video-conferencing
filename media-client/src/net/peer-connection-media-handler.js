import Logger from "../logging/logger";
import StreamIndex from "../models/stream-index";
import FatalErrorHandler from '../handlers/fatal-error-handler'

var logger = new Logger('PeerConnectionMediaHandler');

export default class PeerConnectionMediaHandler {
    /** @type {StreamIndex} */
    _streamIndex = null;

    /** @type {Object} */
    _transceiverMetadataIndexByMid = {};

    /** @type {Object} */
    _transceiverMetadataIndexByDeviceId = {};

    /**
     * @param {StreamIndex} streamIndex 
     */
    constructor(streamIndex) {
        this._streamIndex = streamIndex;
    }

    /**
     * Called whenever we receive transceiver metadata via PeerConnectionOfferer
     * or PeerConnectionAnswerer
     * 
     * @param {Object[]} transceivers 
     */
    setTransceiversMetadata(transceivers) {
        if (!transceivers) {
            throw 'InvalidArguments';
        }

        logger.info('Transceivers metadata received', transceivers);

        // Index the transceiver metadata,
        //  so later in trackAdded() we can query.
        this._transceiverMetadataIndexByMid = {};
        this._transceiverMetadataIndexByDeviceId = {};
        transceivers.forEach(transceiver => this._setTransceiverMetadata(transceiver));

        logger.debug('Transceiver metadata index by mid updated', this._transceiverMetadataIndexByMid);
        logger.debug('Transceiver metadata index by source device id updated', this._transceiverMetadataIndexByDeviceId);

        this._transceiverMetadataChanged();
    }

    /**
     * Called whenever we receive transceiver metadata 
     * 
     * @param {RTCRtpTransceiver} transceiver
     * @param {Object}            transceiverMetadata 
     */
    setTransceiverMetadata(transceiver, transceiverMetadata) {

        logger.info('Transceiver metadata update received', transceiverMetadata);

        // Delete the previous index
        var previous = this._transceiverMetadataIndexByMid[transceiverMetadata.transceiverMid];
        if (previous) {
            delete this._transceiverMetadataIndexByDeviceId[previous.sourceDeviceId];
        }

        // Update our state
        this._setTransceiverMetadata(transceiverMetadata);
        this._transceiverMetadataChanged();
        
        // Update our streamIndex
        this._streamIndex.put(transceiverMetadata.sourceDeviceId, new MediaStream([transceiver.receiver.track]));
    }

    /**
     * Notifies this handler that a track has been added to the provided transceiver
     * 
     * @param {RTCRtpTransceiver} transceiver 
     */
    trackAdded(transceiver, stream) {
        // Get the metadata to find the sourceDeviceId
        // Get the metadata to find the sourceDeviceId
        var metadata = this._transceiverMetadataIndexByMid[transceiver.mid];
        if (!metadata) {
            FatalErrorHandler.failFast(`No transceiver metadata found for mid ${transceiver.mid}`);
        }

        // Update stream index,
        // Note that we can have 2 transceivers for each stream (audio/video),
        // therefore check before updating to avoid unnecessary reloads
        if (this._streamIndex.get(metadata.sourceDeviceId) != stream) {
            this._streamIndex.put(metadata.sourceDeviceId, stream);
        }
    }

    _transceiverMetadataChanged() {
        // Remove the devices in StreamIndex that no longer exist.
        this._streamIndex.getDeviceIds().forEach(deviceId => {
            if (this._transceiverMetadataIndexByDeviceId[deviceId] == undefined) {
                this._streamIndex.remove(deviceId);
            }
        });
    }

    _setTransceiverMetadata(transceiverMetadata) {
        this._transceiverMetadataIndexByMid[transceiverMetadata.transceiverMid] = transceiverMetadata;
        this._transceiverMetadataIndexByDeviceId[transceiverMetadata.sourceDeviceId] = transceiverMetadata;
    }
}