import Logger from '../logging/logger.js'
import ConferenceSettings from '../models/conference-settings.js';
import UserInfo from '../models/user-info.js';

export default class WebSocketClient extends EventTarget
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

    /**
     * @returns {ConferenceSettings}
     */
    get conferenceSettings() {
        return this._conferenceSettings;
    }

    /**
     * @return {UserInfo[]}
     */
    get users() { return this._users; }

    /**
     * @return {String}
     */
    get deviceId() { return this._deviceId; }

    constructor() {
        super();
        this._sendHeartBeat = this._sendHeartBeat.bind(this);
        this._users = [];
        this._messageQueue = [];
    }

    /**
     * @param {ConferenceSettings} conferenceSettings 
     */
    async initializeAsync(conferenceSettings)
    {
        this._logger = new Logger('WebSocketClient');
        this._logger.log(`Initializing WebSocket with settings=${JSON.stringify(conferenceSettings)}`)
        this._conferenceSettings = conferenceSettings;
        this._restart();

        // Start sending heartbeats
        setTimeout(this._sendHeartBeat, 5 * 1000);

        // Promise
        return new Promise((resolve, reject) => {
            this._initializeAsyncResolve = resolve;
            this._initializeAsyncReject = reject;
        });
    }

    queueMessageForSending(command, args) {
        this._messageQueue.push({
            command: command,
            args: args
        });
        this._tryFlueshQueue();
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
        this._webSocket = new WebSocket(webSocketEndpoint);
        // Error
        this.webSocket.addEventListener('error', () => {
            this.logger.error('Error', arguments);
        });
        // Close
        this.webSocket.addEventListener('close', () => {
            this.logger.warn('Close', arguments);
            this.logger.warn('Restaring WebSocket because the connection is closed.');
            setTimeout(() => this._restart(), 1000);
        });
        // Message
        this.webSocket.addEventListener('message', (e) => {
            var response = JSON.parse(e.data);
            this.logger.log('Message', response);
            var commandName = `_on${response.command}`;
            // Do we have a handler? If so, invoke the handler,
            // otherwise trigger an event for externals
            if(typeof this[commandName] == 'undefined')
            {
                this.dispatchEvent(new CustomEvent('message', {
                    detail: {
                        command: commandName,
                        args: response.args
                    }
                }));
            }
            else
                this[commandName](response.args);
        });
        // Open
        this.webSocket.addEventListener('open', () => {
            this.logger.info(`Connected to ${webSocketEndpoint}`);
            this._authenticate();
        });
    }

    _authenticate() {
        this.queueMessageForSending('Authenticate', {});
    }
    
    _onAuthenticationSuccess(args) {
        this.logger.info(`Authentication successful, devieId=${args.deviceId}`);
        this._tryCreateRoom();
        this._deviceId = args.deviceId;
        this.dispatchEvent(new CustomEvent('deviceidchange'));
    }

    _tryCreateRoom() {
        // Send a command to the server to create (if the room doesn't exist)
        this.queueMessageForSending('CreateRoom', {
            newRoomName: this.conferenceSettings.roomId
        });
    }

    _onRoomCreated(roomId) {
        this.logger.info(`Room created ${roomId}`);
        this.queueMessageForSending('JoinRoom', {
            roomId: roomId,
            username: this.conferenceSettings.username
        });
    }

    _onJoinRoomSuccess() {
        this._initializeAsyncResolve();
        this.dispatchEvent(new CustomEvent('room'));
    }

    _onJoinRoomFailed(errorMessage) {
        this._failFast(errorMessage);
    }

    _onRoomCreationFailed(errorMessage) {
        this._failFast(errorMessage);
    }

    _onSync(syncMessage) {
        var tmp = [];
        for(var i in syncMessage.users)
        {
            tmp.push(new UserInfo(syncMessage.users[i]));
        }
        this._users = tmp;
        this._logger.info('Synced', this.users);
        this.dispatchEvent(new CustomEvent('users', {
            detail: this.users
        }));
    }

    _failFast(errorMessage) {
        this._initializeAsyncReject(errorMessage);
    }

    _sendHeartBeat() {
        if(this.webSocket.readyState == WebSocket.OPEN)
        {
            this._send('HeartBeat', { timestamp: new Date().getTime()/1000 });
        }
        setTimeout(this._sendHeartBeat, 5 * 1000);
    }

    _tryFlueshQueue() {
        // Dump all the messages as long as the WS is open
        while(this.webSocket.readyState == WebSocket.OPEN
            && this._messageQueue.length > 0)
        {
            var message = this._messageQueue[0];
            this._messageQueue = this._messageQueue.splice(1);
            this._send(message.command, message.args);
        }
    }

    _send(command, args)
    {
        this.webSocket.send(JSON.stringify({
            command: command,
            args: args
        }));         
        if(command != 'HeartBeat')
            this.logger.log('WebSocket command sent', command, args);
    }
}