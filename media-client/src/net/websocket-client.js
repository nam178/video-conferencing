import Logger from '../logging/logger.js'
import ConferenceSettings from '../models/converence-settings.js';

export default class WebSocketClient
{
    /**
     * @returns {Logger}
     */
    get logger() { 
        return this._logger; 
    }

    /**
     * @returns {WebSocket}
     */
    get webSocket() { return this._webSocket; }
    set webSocket(value) { this._webSocket = value; }

    /**
     * @returns {ConferenceSettings}
     */
    get conferenceSettings() {
        return this._conferenceSettings;
    }

    constructor() {
        this._sendHeartBeat = this._sendHeartBeat.bind(this);
    }

    /**
     * @param {ConferenceSettings} conferenceSettings 
     */
    initialize(conferenceSettings)
    {
        this._logger = new Logger('WebSocketClient');
        this._conferenceSettings = conferenceSettings;
        this._restart();

        // Start sending heartbeats
        setTimeout(this._sendHeartBeat, 5 * 1000);
    }

    _restart()
    {
        // Close any existing WebSocket connections
        if(this.webSocket 
            && this.webSocket.readyState != WebSocket.CLOSING 
            && this.webSocket.readyState != WebSocket.CLOSED)
        {
            this.webSocket.close();
        }

        // Start a new one
        var webSocketEndpoint = `ws://${CONF_SERVER_HOST}:${CONF_SERVER_PORT}`;
        this.logger.log(`Connecting to ${webSocketEndpoint}..`)
        this.webSocket = new WebSocket(webSocketEndpoint);
        this.webSocket.addEventListener('error', () => {
            this.logger.error('Error', arguments);
        });
        this.webSocket.addEventListener('close', () => {
            this.logger.warn('Close', arguments);
            this.logger.warn('Restaring WebSocket because the connection is closed.');
            setTimeout(() => this._restart(), 1000);
        });
        this.webSocket.addEventListener('message', () => {
            this.logger.log('Message', arguments);
        });
        this.webSocket.addEventListener('open', () => {
            this.logger.info(`Connected to ${webSocketEndpoint}`)
        });
    }

    _sendHeartBeat() 
    {
        if(this.webSocket.readyState == WebSocket.OPEN)
        {
            this._send('HeartBeat', { timestamp: new Date().getTime()/1000 });
        }
        setTimeout(this._sendHeartBeat, 5 * 1000);
    }

    _send(command, args)
    {
        this.webSocket.send(JSON.stringify({
            command: command,
            args: args
        }));         
        console.log('WebSocket command sent', command, args);
    }
}