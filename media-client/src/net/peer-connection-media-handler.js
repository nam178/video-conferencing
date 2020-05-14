import Logger from "../logging/logger";
import StreamIndex from "../models/stream-index";

var logger = new Logger('PeerConnectionMediaHandler');

export default class PeerConnectionMediaHandler {
    /** @type {StreamIndex} */
    _streamIndex = null;

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
        if(!transceivers) {
            throw 'InvalidArguments';
        }
        logger.info('Transceivers metadata received', transceivers);
    }
}