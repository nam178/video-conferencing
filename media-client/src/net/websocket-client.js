export default class WebSocketClient
{
    static get webSocket() {
        return this._webSocket;
    }

    static set webSocket(value) {
        console.log('setter')
        this._webSocket = value;
    }

    static initialize()
    {
        this.webSocket = new WebSocket(`ws://${CONF_SERVER_HOST}:${CONF_SERVER_PORT}`);
    }
}

WebSocketClient.initialize();