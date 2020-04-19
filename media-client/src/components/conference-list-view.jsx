import React from 'react';
import ConferenceListCell from './conference-list-cell.jsx';
import DeviceSelectButton from './device-select-button.jsx';
import InputDeviceManager from '../models/input-device-manager.js';
import PeerConnectionManager from '../net/peer-connection-manager.js';
import './conference-list-view.css'

export default class ConferenceListView extends React.Component {
    /** @returns {InputDeviceManager} */
    get deviceManager() { return this._deviceManager; }

    /** @returns {PeerConnectionManager} */
    get peerConnection() { return this._peerConnectionManager; }

    constructor() {
        super();
        this.state = {
            users: null,
            microphoneDevices: null,
            cameraDevices: null,
            speakerDevices: null,
            streams: { }
        };
        this._deviceManager = new InputDeviceManager();
        this.handleMicrophoneChange = this.handleMicrophoneChange.bind(this);
        this.handleCameraChange = this.handleCameraChange.bind(this);
        this.handleSpeakerChange = this.handleSpeakerChange.bind(this);
        this.handleDeviceClick = this.handleDeviceClick.bind(this);
    }

    async componentDidMount() {
        // Restore previous preferences
        var lastAudioDeviceId = localStorage.getItem('conference_list_view_audio_device_id');
        var lastVideoDeviceId = localStorage.getItem('conference_list_view_video_device_id');
        var lastAudioSinkId = localStorage.getItem('conference_list_view_audio_sink_id');
        if (lastAudioDeviceId)
            this.deviceManager.currentAudioInputDeviceId = lastAudioDeviceId;
        if (lastVideoDeviceId)
            this.deviceManager.currentVideoInputDeviceId = lastVideoDeviceId;
        if (lastAudioSinkId)
            this.deviceManager.currentOutAudioSinkId = lastAudioSinkId;
        this._peerConnectionManager = new PeerConnectionManager(this.props.webSocketClient);
        await this.reInitializeDevicesAsync();
    }

    async componentWillUnmount() {
        this._closed = true;
    }

    async reInitializeDevicesAsync() {
        try {
            this._isInitialisingDevices = true;
            await this.deviceManager.initializeAsync();
            // Device initialisation successed,
            // First, update peerConnection
            this._peerConnectionManager.localMediaStreamForSending = this.deviceManager.stream;
            // Then, update the UI
            if(!this._closed) {
                var streamsCopy = {};
                for(var k in this.state.streams) {
                    streamsCopy[k] = this.state.streams[k];
                }
                if(this.deviceManager.stream) {
                    streamsCopy[this.state.me.id] = this.deviceManager.stream;
                }
                this.setState({
                    isVideoLoading: false,
                    isAudioLoading: false,
                    microphoneDevices: this.generateDropDownItem('audioinput'),
                    cameraDevices: this.generateDropDownItem('videoinput'),
                    speakerDevices: this.generateDropDownItem('audiooutput'),
                    streams: streamsCopy
                });
            }
        }
        // On any error related to initialising devices,
        // display a warning. We can still use the room without any devices.
        // TODO display better alert
        catch (errorMessage) {
            if (!this._closed) {
                window.alert(`Device initialisation error: ${errorMessage}`);
            }
        }
        finally {
            this._isInitialisingDevices = false;
        }
    }

    async handleMicrophoneChange(item) {
        if(!item.id) { ;return; } // Don't do anything when query device fail (it returns an empty device with id='')
        this.deviceManager.currentAudioInputDeviceId = item.id;
        this.rememberAudioChoice();
        await this.reInitializeDevicesAsync();
    }

    async handleCameraChange(item) {
        if(!item.id) { ;return; } // Don't do anything when query device fail (it returns an empty device with id='')
        this.deviceManager.currentVideoInputDeviceId = item.id;
        this.rememberVideoChoice();
        await this.reInitializeDevicesAsync();
    }

    handleSpeakerChange(item) {
        if(!item.id) { ;return; } // Don't do anything when query device fail (it returns an empty device with id='')
        this.deviceManager.currentOutAudioSinkId = item.id;
        this.rememberSpeakerChoice();
        this.setState({
            speakerDevices: this.generateDropDownItem('audiooutput')
        });
    }

    handleDeviceClick(sender) {
        // Devices are initialising, prevent showing the device selector
        if (this._isInitialisingDevices) {
            return false;
        }
        // When the the devices disabled, clicking the device button won't show
        // any device to select, therefore we'll re-initialise the devices first
        if (sender == this._audioSelectButton
            && this.deviceManager.currentAudioInputDeviceId == InputDeviceManager.NotSelectedDeviceId()) {
            this.deviceManager.currentAudioInputDeviceId = null;
            this.rememberAudioChoice();
            this.setState({ isAudioLoading: true });
            this.reInitializeDevicesAsync(); // fire and forget
            return false;
        }
        if (sender == this._videoSelectButton
            && this.deviceManager.currentVideoInputDeviceId == InputDeviceManager.NotSelectedDeviceId()) {
            this.deviceManager.currentVideoInputDeviceId = null;
            this.rememberVideoChoice();
            this.setState({ isVideoLoading: true });
            this.reInitializeDevicesAsync(); // fire and forget
            return false;
        }
        return true;
    }

    generateDropDownItem(kind) {
        var result = [];
        var number = 1;
        var selectedDeviceId = null;
        if (kind == 'audioinput')
            selectedDeviceId = this.deviceManager.currentAudioInputDeviceId;
        if (kind == 'videoinput')
            selectedDeviceId = this.deviceManager.currentVideoInputDeviceId;
        if (kind == 'audiooutput') {
            selectedDeviceId = this.deviceManager.currentOutAudioSinkId;
            if (!selectedDeviceId) {
                var tmp = this.deviceManager.mediaDevices.find(d => d.deviceId == 'default');
                if (tmp) {
                    selectedDeviceId = tmp.deviceId;
                }
            }
        }

        this.deviceManager.mediaDevices.forEach(device => {
            if (device.kind == kind) {
                result.push({
                    id: device.deviceId,
                    name: device.label || ConferenceListView.getDefaultName(number, kind),
                    selected: device.deviceId == selectedDeviceId
                })
                number++;
            }
        });
        if (kind != 'audiooutput') {
            result.push({
                id: InputDeviceManager.NotSelectedDeviceId(),
                name: 'Don\'t Use',
                selected: selectedDeviceId == InputDeviceManager.NotSelectedDeviceId()
            });
        } else if(result.length == 1) {
            // special case for audiooutput:
            // when it fails to get a list of audiooutput devices,
            // it will returns just one empty device with deviceId is empty,
            // to avoid confusion to the users, we'll mark that device as selected,
            // and won't do anything when the user click on it (see handler above)
            result[0].selected = true;
        }
        return result;
    }

    rememberAudioChoice() {
        this.remember('conference_list_view_audio_device_id', this.deviceManager.currentAudioInputDeviceId);
    }

    rememberVideoChoice() {
        this.remember('conference_list_view_video_device_id', this.deviceManager.currentVideoInputDeviceId);
    }

    rememberSpeakerChoice() {
        this.remember('conference_list_view_audio_sink_id', this.deviceManager.currentOutAudioSinkId);
    }

    remember(key, value) {
        if (value)
            localStorage.setItem(key, value);
        else
            localStorage.removeItem(key);
    }

    static getDefaultName(number, kind) {
        switch (kind) {
            case 'audioinput':
                return `Microphone ${number}`;
            case 'videoinput':
                return `Camera ${number}`;
            case 'audiooutput':
                return `Speaker ${number}`;
        }
    }

    static getDerivedStateFromProps(props) {
        return {
            users: props.users.sort((x, y) => {
                // Show my self first
                if (x.username == props.webSocketClient.conferenceSettings.username)
                    return -1;
                if (y.username == props.webSocketClient.conferenceSettings.username)
                    return 1;
                // Show offline people at the bottom
                if (!x.isOnline)
                    return 1;
                if (!y.isOnline)
                    return -1;
                // The rest sort by username
                return x.username.localeCompare(y.username);
            }),
            me: props.users.find(u => u.username == props.webSocketClient.conferenceSettings.username)
        };
    }

    render() {
        return <div className="conference-list-view">
            <div className="heading">
                <div>#{this.props.webSocketClient.conferenceSettings.roomId}</div>
            </div>
            <div className="margin-fix">
                <div className="body">
                    {this.state.users.map(user => <ConferenceListCell 
                        stream={typeof(this.state.streams[user.id]) != 'undefined' ? this.state.streams[user.id] : null}
                        key={user.id} 
                        user={user} 
                        self={user.username == this.props.webSocketClient.conferenceSettings.username} /> )}
                </div>
            </div>
            <div className="padding"></div>
            <div className="bottom-bar">
                {this.state.microphoneDevices ? <DeviceSelectButton ref={t => this._audioSelectButton = t}
                    onClick={this.handleDeviceClick}
                    isLoading={this.state.isAudioLoading}
                    icon={this.deviceManager.currentAudioInputDeviceId == InputDeviceManager.NotSelectedDeviceId() ? 'microphone-slash' : 'microphone'}
                    gray={this.deviceManager.currentAudioInputDeviceId == InputDeviceManager.NotSelectedDeviceId()}
                    selectItems={this.state.microphoneDevices}
                    title="Which Microphone?"
                    onItemClick={this.handleMicrophoneChange} /> : null}
                {this.state.cameraDevices ? <DeviceSelectButton ref={t => this._videoSelectButton = t}
                    onClick={this.handleDeviceClick}
                    isLoading={this.state.isVideoLoading}
                    icon={this.deviceManager.currentVideoInputDeviceId == InputDeviceManager.NotSelectedDeviceId() ? 'camera' : 'camera'}
                    gray={this.deviceManager.currentVideoInputDeviceId == InputDeviceManager.NotSelectedDeviceId()}
                    selectItems={this.state.cameraDevices}
                    title="Which Camera?"
                    onItemClick={this.handleCameraChange} /> : null}
                {this.state.speakerDevices ? <DeviceSelectButton ref={t => this._speakerSelectButton = t}
                    onClick={this.handleDeviceClick}
                    isLoading={this.state.isSpeakerLoading}
                    icon="headset"
                    selectItems={this.state.speakerDevices}
                    title="Which Speaker/Headset?"
                    onItemClick={this.handleSpeakerChange} /> : null}
            </div>
        </div>
    }
}