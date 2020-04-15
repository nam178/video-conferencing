import React from 'react';
import ConferenceListCell from './conference-list-cell.jsx';
import DeviceSelectButton from './device-select-button.jsx';
import InputDevices from '../models/input-devices.js';
import './conference-list-view.css'

export default class ConferenceListView extends React.Component {
    /** @returns {InputDevices} */
    get inputDevices() { return this._inputDevices; }

    constructor() {
        super();
        this.state = {
            users: null,
            microphoneDevices: null,
            cameraDevices: null,
            speakerDevices: null
        };
        this._inputDevices = new InputDevices();
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
            this.inputDevices.currentAudioInputDeviceId = lastAudioDeviceId;
        if (lastVideoDeviceId)
            this.inputDevices.currentVideoInputDeviceId = lastVideoDeviceId;
        if (lastAudioSinkId)
            this.inputDevices.currentOutAudioSinkId = lastAudioSinkId;
        // Then initialise
        await this.reInitializeDevicesAsync();
    }

    async componentWillUnmount() {
        this._closed = true;
    }

    async reInitializeDevicesAsync() {
        try {
            this._isInitialisingDevices = true;
            await this.inputDevices.initializeAsync();
        }
        catch (errorMessage) {
            // TODO display better alert
            if (!this._closed) {
                window.alert('Warning: ' + errorMessage);
            }
            return;
        }
        finally {
            this._isInitialisingDevices = false;
        }
        if (!this._closed) {
            // Successed, update devices
            this.setState({
                isVideoLoading: false,
                isAudioLoading: false,
                microphoneDevices: this.generateDropDownItem('audioinput'),
                cameraDevices: this.generateDropDownItem('videoinput'),
                speakerDevices: this.generateDropDownItem('audiooutput')
            });
        }
    }

    async handleMicrophoneChange(item) {
        if(!item.id) { ;return; } // Don't do anything when query device fail (it returns an empty device with id='')
        this.inputDevices.currentAudioInputDeviceId = item.id;
        this.rememberAudioChoice();
        await this.reInitializeDevicesAsync();
    }

    async handleCameraChange(item) {
        if(!item.id) { ;return; } // Don't do anything when query device fail (it returns an empty device with id='')
        this.inputDevices.currentVideoInputDeviceId = item.id;
        this.rememberVideoChoice();
        await this.reInitializeDevicesAsync();
    }

    handleSpeakerChange(item) {
        if(!item.id) { ;return; } // Don't do anything when query device fail (it returns an empty device with id='')
        this.inputDevices.currentOutAudioSinkId = item.id;
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
            && this.inputDevices.currentAudioInputDeviceId == InputDevices.NotSelectedDeviceId()) {
            this.inputDevices.currentAudioInputDeviceId = null;
            this.rememberAudioChoice();
            this.setState({ isAudioLoading: true });
            this.reInitializeDevicesAsync(); // fire and forget
            return false;
        }
        if (sender == this._videoSelectButton
            && this.inputDevices.currentVideoInputDeviceId == InputDevices.NotSelectedDeviceId()) {
            this.inputDevices.currentVideoInputDeviceId = null;
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
            selectedDeviceId = this.inputDevices.currentAudioInputDeviceId;
        if (kind == 'videoinput')
            selectedDeviceId = this.inputDevices.currentVideoInputDeviceId;
        if (kind == 'audiooutput') {
            selectedDeviceId = this.inputDevices.currentOutAudioSinkId;
            if (!selectedDeviceId) {
                var tmp = this.inputDevices.mediaDevices.find(d => d.deviceId == 'default');
                if (tmp) {
                    selectedDeviceId = tmp.deviceId;
                }
            }
        }

        this.inputDevices.mediaDevices.forEach(device => {
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
                id: InputDevices.NotSelectedDeviceId(),
                name: 'Don\'t Use',
                selected: selectedDeviceId == InputDevices.NotSelectedDeviceId()
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
        this.remember('conference_list_view_audio_device_id', this.inputDevices.currentAudioInputDeviceId);
    }

    rememberVideoChoice() {
        this.remember('conference_list_view_video_device_id', this.inputDevices.currentVideoInputDeviceId);
    }

    rememberSpeakerChoice() {
        this.remember('conference_list_view_audio_sink_id', this.inputDevices.currentOutAudioSinkId);
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
                if (x.username == props.conferenceSettings.username)
                    return -1;
                if (y.username == props.conferenceSettings.username)
                    return 1;
                // Show offline people at the bottom
                if (!x.isOnline)
                    return 1;
                if (!y.isOnline)
                    return -1;
                // The rest sort by username
                return x.username.localeCompare(y.username);
            })
        };
    }

    render() {
        return <div className="conference-list-view">
            <div className="heading">
                <div>#{this.props.conferenceSettings.roomId}</div>
            </div>
            <div className="margin-fix">
                <div className="body">
                    {this.state.users.map(user => <ConferenceListCell key={user.id} user={user} self={user.username == this.props.conferenceSettings.username} />)}
                </div>
            </div>
            <div className="padding"></div>
            <div className="bottom-bar">
                {this.state.microphoneDevices ? <DeviceSelectButton ref={t => this._audioSelectButton = t}
                    onClick={this.handleDeviceClick}
                    isLoading={this.state.isAudioLoading}
                    icon={this.inputDevices.currentAudioInputDeviceId == InputDevices.NotSelectedDeviceId() ? 'microphone-slash' : 'microphone'}
                    gray={this.inputDevices.currentAudioInputDeviceId == InputDevices.NotSelectedDeviceId()}
                    selectItems={this.state.microphoneDevices}
                    title="Which Microphone?"
                    onItemClick={this.handleMicrophoneChange} /> : null}
                {this.state.cameraDevices ? <DeviceSelectButton ref={t => this._videoSelectButton = t}
                    onClick={this.handleDeviceClick}
                    isLoading={this.state.isVideoLoading}
                    icon={this.inputDevices.currentVideoInputDeviceId == InputDevices.NotSelectedDeviceId() ? 'camera' : 'camera'}
                    gray={this.inputDevices.currentVideoInputDeviceId == InputDevices.NotSelectedDeviceId()}
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