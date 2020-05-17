import WebSocketClient from './websocket-client.js';
import ConferenceSettings from '../models/conference-settings.js';
import InputDeviceManager from './input-device-manager.js';
import UserInfo from '../models/user-info.js';
import StreamIndex from '../models/stream-index'
import Logger from '../logging/logger.js';
import PeerConnectionController from './peer-connection-controller'
import EventTarget2 from '../utils/events.js';

export let NotSelectedInputDeviceId = -1;

export class InputDeviceManagerState {
    static NotInitialised = 0;
    static Scanning = 1;
    static Error = 2;
    static Ok = 3;
}

/**
 * @event MediaClient#user
 */
export default class MediaClient extends EventTarget2 {
    /**
     * @type {WebSocketClient}
     */
    _webSocketClient;

    /**
     * @type {PeerConnectionController}
     */
    _peerConnectionController;

    /**
     * @type {InputDeviceManager}
     */
    _inputDeviceManager = new InputDeviceManager();

    /**
     * @type {Object}
     */
    _streams = {};

    /**
     * @type {Boolean}
     */
    _isInitialised = false;

    /**
     * @var {StreamIndex}
     */
    _streamIndex = new StreamIndex();

    /**
     * @var {Logger}
     */
    _logger = new Logger('MediaClient');

    /**
     * @returns {String}
     */
    get selectedAudioInputDeviceId() { return this._inputDeviceManager.currentAudioInputDeviceId; }

    /**
     * @return {MediaDeviceInfo[]}
     */
    get mediaDevices() { return this._inputDeviceManager.mediaDevices; }

    /**
     * @returns {UserInfo[]}
     */
    get users() { return this._webSocketClient.users; }

    /**
     * @param {String} value
     */
    set selectedAudioInputDeviceId(value) {
        if (this._inputDeviceManagerState == InputDeviceManagerState.Scanning) {
            throw 'InvalidOperationDevicesInitialising';
        }
        this._inputDeviceManager.currentAudioInputDeviceId = value;
        this._rememberAudioChoice();
        this._scanDevicesAsync();
    }

    /**
     * @returns {String}
     */
    get selectedVideoInputDeviceId() { return this._inputDeviceManager.currentVideoInputDeviceId; }

    /**
     * @params {String} value
     */
    set selectedVideoInputDeviceId(value) {
        if (this._inputDeviceManagerState == InputDeviceManagerState.Scanning) {
            throw 'InvalidOperationDevicesInitialising';
        }
        this._inputDeviceManager.currentVideoInputDeviceId = value;
        this._rememberVideoChoice();
        this._scanDevicesAsync();
    }

    /**
     * @returns {String}
     */
    get selectedAudioSinkId() { return this._inputDeviceManager.selectedAudioSinkId; }

    /**
     * @returns {String}
     */
    set selectedAudioSinkId(value) {
        if (this._inputDeviceManagerState == InputDeviceManagerState.Scanning) {
            throw 'InvalidOperationDevicesInitialising';
        }
        this._inputDeviceManager.selectedAudioSinkId = value;
        this._rememberSpeakerChoice();
        this._scanDevicesAsync();
    }

    /**
     * @return {Number}
     */
    get inputDeviceManagerState() { return this._inputDeviceManagerState; }
    _inputDeviceManagerState = InputDeviceManagerState.NotInitialised;

    /**
     * @return {ConferenceSettings}
     */
    get conferenceSettings() { return this._webSocketClient.conferenceSettings; }

    constructor() {
        super();
        this._handleWebSocketClientUsersChange = this._handleWebSocketClientUsersChange.bind(this);
        this._handleInputDeviceStreamChange = this._handleInputDeviceStreamChange.bind(this);
        this._handleMyNetworkDeviceIdChange = this._handleMyNetworkDeviceIdChange.bind(this);
        this._rebuildStreams = this._rebuildStreams.bind(this);

        this._webSocketClient = new WebSocketClient();
        this._peerConnectionController = new PeerConnectionController(this._webSocketClient, this._streamIndex);
        this._webSocketClient.addEventListener('users', this._handleWebSocketClientUsersChange);
        this._webSocketClient.addEventListener('deviceidchange', this._handleMyNetworkDeviceIdChange);
        this._inputDeviceManager.addEventListener('streamchange', this._handleInputDeviceStreamChange);
        this._streamIndex.addEventListener('changed', this._rebuildStreams);
    }

    /**
     * Initialise this client, caller should call this only once.
     * @param {ConferenceSettings} settings 
     */
    async initializeAsync(settings) {
        if (this._isInitialised) {
            throw 'AlreadyInitialized';
        }
        this._isInitialised = true;

        // First initialise device, 
        // so that at the time we create PeerConnection, devices are ready,
        // avoid extra negotations (when already finished negotiating then devices become ready)
        try{
             // Restore previous device selection preferences
            var lastAudioDeviceId = localStorage.getItem('media_client_last_audio_device_id');
            var lastVideoDeviceId = localStorage.getItem('media_client_last_video_device_id');
            var lastAudioSinkId = localStorage.getItem('media_client_last_audio_sink_id');
            if (lastAudioDeviceId)
                this._inputDeviceManager.currentAudioInputDeviceId = lastAudioDeviceId;
            if (lastVideoDeviceId)
                this._inputDeviceManager.currentVideoInputDeviceId = lastVideoDeviceId;
            if (lastAudioSinkId)
                this._inputDeviceManager.selectedAudioSinkId = lastAudioSinkId;

            await this._scanDevicesAsync();
        }
        finally
        {
            // Then initialise network connections (always, even
            // when intialising devices fail, i.e. user can still join
            // the room and watch without no devices)
            await this._webSocketClient.initializeAsync(settings);
        }
    }

    /**
     * The mapping between network device id -> stream, can be used to display videos on the UI.
     * 
     * @returns {Object}
     */
    generateStreams() {
        return this._streams;
    }

    _handleWebSocketClientUsersChange(e) {
        this._rebuildStreams();
        this.dispatchEvent('users');
    }

    _handleInputDeviceStreamChange(e) {
        this._peerConnectionController.localStream = this._inputDeviceManager.stream;
        this._rebuildStreams();
    }

    _handleMyNetworkDeviceIdChange(e) {
        this._rebuildStreams();
    }

    _rebuildStreams() {
        this._streams = {};
        this._webSocketClient.users.forEach(u => {
            u.devices.forEach(device => {
                this._streams[device.id] = device.id == this._webSocketClient.deviceId
                    ? this._inputDeviceManager.stream
                    : this._streamIndex.get(device.id)
            });
        });
        this._logger.info('Streams updated', this._streams);
        this.dispatchEvent('streams');
    }

    _rememberAudioChoice() {
        this._remember('media_client_last_audio_device_id', this.selectedAudioInputDeviceId);
    }

    _rememberVideoChoice() {
        this._remember('media_client_last_video_device_id', this.selectedVideoInputDeviceId);
    }

    _rememberSpeakerChoice() {
        this._remember('media_client_last_audio_sink_id', this.selectedAudioSinkId);
    }

    _remember(key, value) {
        if (value)
            localStorage.setItem(key, value);
        else
            localStorage.removeItem(key);
    }

    async _scanDevicesAsync() {
        if (this._inputDeviceManagerState == InputDeviceManagerState.Scanning) {
            throw 'AlreadyInitializingDevices';
        }
        this._inputDeviceManagerState = InputDeviceManagerState.Scanning;
        this.dispatchEvent('device-scanning-started');
        try {
            await this._inputDeviceManager.requestAccessToDevicesAsync();
            this._inputDeviceManagerState = InputDeviceManagerState.Ok;
        }
        catch (err) {
            // TODO display better error message UI/UI, etc..
            window.alert(err);
            this._inputDeviceManagerState = InputDeviceManagerState.Error;
        }
        finally {
            this.dispatchEvent('device-scanning-completed');
        }
    }
}
