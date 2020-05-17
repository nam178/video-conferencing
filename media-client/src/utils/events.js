export default class EventTarget2
{
    _eventListeners = {};

    addEventListener(type, handler) {
        if(this._eventListeners[type] == undefined)
        {
            this._eventListeners[type] = [];
        }

        this._eventListeners[type].push(handler);
    }

    removeEventListener(type, handler)
    {
        if(this._eventListeners[type])
        {
            this._eventListeners[type] = this._eventListeners[type].filter(i => i != handler);

            if(this._eventListeners[type].length == 0)
            {
                delete this._eventListeners[type];
            }
        }
    }

    dispatchEvent(type, args) {
        if(this._eventListeners[type])
        {
            this._eventListeners[type].forEach(handler => handler(args));
        }
    }
}