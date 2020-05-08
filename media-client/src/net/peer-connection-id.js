export default class PeerConnectionId {
    /**
     * @returns {Boolean}
     */
    get hasValue() { return !!this._value; }

    /**
     * @returns {String}
     */
    get value() { return this._value; }

    /**
     * @param {String} value
     */
    set value(value) { this._value = value; }
}
