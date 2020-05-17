import Logger from "../logging/logger";
import EventTarget2 from "../utils/events";

var logger = new Logger('StreamIndex');

export default class StreamIndex extends EventTarget2
{
    _index = {};

    /**
     * @param {String} deviceId 
     * @param {MediaStream} stream 
     */
    put(deviceId, stream) {
        this._index[deviceId] = stream;
        logger.debug(`Added stream ${stream.id}`);
        this.dispatchEvent('changed');
    }

    /**
     * @param {String} deviceId 
     * @returns {MediaStream}
     */
    get(deviceId) {
        if(this._index[deviceId]) {
            return this._index[deviceId];
        }
        return null;
    }

    /**
     * @param {String} deviceId 
     */
    remove(deviceId) {
        if(this._index[deviceId]) {
            delete this._index[deviceId];
            logger.debug(`Removed stream ${deviceId}`);
            this.dispatchEvent('changed');
        }
    }

    /**
     * @returns {String[]}
     */
    getDeviceIds() {
        var deviceIds = [];

        for(var i in this._index) {
            deviceIds.push(i);
        }

        return deviceIds;
    }
}