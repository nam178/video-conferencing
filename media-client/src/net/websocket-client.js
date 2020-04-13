import Logger from '../logging/logger.js'
import ConferenceSettings from '../models/conference-settings.js';
import UserProfile from '../models/user-profile.js';

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
     * @return {UserProfile[]}
     */
    get users() { return this._users; }

    constructor() {
        super();
        this._sendHeartBeat = this._sendHeartBeat.bind(this);
        this._users = [];
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
            
            if(typeof this[commandName] == 'undefined')
                this.logger.error(`${commandName} handler was not implemented`);
            else
                this[commandName](response.args);
        });
        // Open
        this.webSocket.addEventListener('open', () => {
            this.logger.info(`Connected to ${webSocketEndpoint}`)
            this._tryCreateRoom();
        });
    }

    _tryCreateRoom() {
        // Send a command to the server to create (if the room doesn't exist)
        this._send('CreateRoom', {
            newRoomName: this.conferenceSettings.roomId
        });
    }

    _onRoomCreated(roomId) {
        this.logger.info(`Room created ${roomId}`);
        this._send('JoinRoom', {
            roomId: roomId,
            username: this.conferenceSettings.username
        });
    }

    _onJoinRoomSuccess() {
        this._initializeAsyncResolve();
    }

    _onJoinRoomFailed(errorMessage) {
        failFast(errorMessage);
    }

    _onRoomCreationFailed(errorMessage) {
        failFast(errorMessage);
    }

    _onUpdateUsers(users) {
        var tmp = [];
        for(var i in users.users)
        {
            tmp.push(new UserProfile(users.users[i]));
        }
        this._users = tmp;
        this._logger.info('Users updated', this.users);
        this.dispatchEvent(new CustomEvent('users', {
            detail: this.users
        }));
    }

    _failFast(errorMessage) {
        this._initializeAsyncReject(errorMessage);
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
        if(command != 'HeartBeat')
            this.logger.log('WebSocket command sent', command, args);
    }
}