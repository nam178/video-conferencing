import WebSocketClient from './websocket-client.js';
import Logger from '../logging/logger.js'

export default class WebSocketMessageHandler {
    /**
     * @type {WebSocketClient}
     */
    _webSocketClient = null;
    
    /**
     * @type {Logger}
     */
    _logger = null;

    /**
     * @returns {WebSocketClient}
     */
    get webSocketClient() {
        return this._webSocketClient;
    }

    constructor(webSocketClient, logger) {
        this._webSocketClient = webSocketClient;
        this._logger = logger;
        this._handleWebSocketMessage = this._handleWebSocketMessage.bind(this);
    }

    startObservingWebSocketMessages() {
        this._webSocketClient.addEventListener('message', this._handleWebSocketMessage);
    }

    stopObservingWebSocketMessages() {
        this._webSocketClient.removeEventListener('message', this._handleWebSocketMessage);
    }

    _handleWebSocketMessage(e) {
        var commandName = `_on${e.detail.command}`;
        if (typeof this[commandName] != 'undefined') {
            this[commandName](e.detail.args);
        }
    }
}
