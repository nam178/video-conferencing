import Logger from "../logging/logger";

var logger = new Logger('StreamIndex');

export default class StreamIndex extends EventTarget
{
    _index = {};

    put(stream) {
        this._index[stream.id] = stream;
        logger.debug(`Added stream ${stream.id}`);
        this.dispatchEvent(new CustomEvent('changed'));
    }

    get(streamId) {
        if(this._index[streamId]) {
            return this._index[streamId];
        }
        return null;
    }

    remove(streamId) {
        if(this._index[streamId]) {
            delete this._index[streamId];
            logger.debug(`Removed stream ${streamId}`);
            this.dispatchEvent(new CustomEvent('changed'));
        }
    }
}