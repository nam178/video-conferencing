export default class Throttle {

    /** @type {Number} */
    _timeout = 0;

    /**
     * @param {Number} timeout 
     */
    constructor(timeout) {
        this._timeout = timeout;
    }

    /**
     * @param {Function} action
     */
    run(action) {
        if(this._handle) {
            window.clearTimeout(this._handle);
        }
        this._handle = window.setTimeout(action, this._timeout);
    }
}