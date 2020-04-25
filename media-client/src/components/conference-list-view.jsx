import React from 'react';
import ConferenceListCell from './conference-list-cell.jsx';
import DeviceSelectButton from './device-select-button.jsx';
import { NotSelectedInputDeviceId, InputDeviceManagerState } from '../net/media-client.js';
import './conference-list-view.css'

export default class ConferenceListView extends React.Component {
    constructor() {
        super();
        this.state = {
            users: [],
            isAudioLoading: false,
            isVideoLoading: false,
            isSpeakerLoading: false,
            microphoneDevices: null,
            cameraDevices: null,
            speakerDevices: null,
            streams: {}
        };
        this.handleMicrophoneChange = this.handleMicrophoneChange.bind(this);
        this.handleCameraChange = this.handleCameraChange.bind(this);
        this.handleSpeakerChange = this.handleSpeakerChange.bind(this);
        this.handleDeviceClick = this.handleDeviceClick.bind(this);
        this.handleUsersChange = this.handleUsersChange.bind(this);
        this.handleStreamsUpdate = this.handleStreamsUpdate.bind(this);
        this.handleInputDeviceManagerStateChange = this.handleInputDeviceManagerStateChange.bind(this);
    }

    async componentDidMount() {
        this.props.mediaClient.addEventListener('streams', this.handleStreamsUpdate);
        this.props.mediaClient.addEventListener('users', this.handleUsersChange);
        this.props.mediaClient.addEventListener('device-scanning-started', this.handleInputDeviceManagerStateChange);
        this.props.mediaClient.addEventListener('device-scanning-completed', this.handleInputDeviceManagerStateChange);
        // Set initial state
        this.handleUsersChange();
        this.handleInputDeviceManagerStateChange();
        this.handleStreamsUpdate();
    }

    async componentWillUnmount() {
        this._closed = true;
        this.props.mediaClient.removeEventListener('streams', this.handleStreamsUpdate);
    }

    handleUsersChange() {
        var users = this.props.mediaClient.users ?? [];
        this.setState({
            users: users.sort((x, y) => {
                // Show my self first
                if (x.username == this.props.mediaClient.conferenceSettings.username)
                    return -1;
                if (y.username == this.props.mediaClient.conferenceSettings.username)
                    return 1;
                // Show offline people at the bottom
                if (!x.isOnline)
                    return 1;
                if (!y.isOnline)
                    return -1;
                // The rest sort by username
                return x.username.localeCompare(y.username);
            })
        });
    }

    handleInputDeviceManagerStateChange() {
        switch (this.props.mediaClient.inputDeviceManagerState) {
            case InputDeviceManagerState.NotInitialised:
            case InputDeviceManagerState.Error:
                // This will not show the buttons at all
                this.setState({
                    microphoneDevices: null,
                    cameraDevices: null,
                    speakerDevices: null
                });
                break;
            case InputDeviceManagerState.Scanning:
                // This will show the buttons,
                // however they have no items.
                // Nothing will happen when user clicks on them, as we checked below.
                this.setState({
                    microphoneDevices: [],
                    cameraDevices: [],
                    speakerDevices: []
                });
                break;
            case InputDeviceManagerState.Ok:
                // This will show the buttons, and they will have items to click on.
                this.setState({
                    microphoneDevices: this.generateDropDownItem('audioinput'),
                    cameraDevices: this.generateDropDownItem('videoinput'),
                    speakerDevices: this.generateDropDownItem('audiooutput'),
                    isVideoLoading: false,
                    isAudioLoading: false,
                    isSpeakerLoading: false,
                });
                break;
        }
    }

    handleStreamsUpdate() {
        this.setState({ streams: this.props.mediaClient.generateStreams() });
    }

    async handleMicrophoneChange(item) {
        if (!item.id) { ; return; } // Don't do anything when query device fail (it returns an empty device with id='')
        this.props.mediaClient.selectedAudioInputDeviceId = item.id;
    }

    async handleCameraChange(item) {
        if (!item.id) { ; return; } // Don't do anything when query device fail (it returns an empty device with id='')
        this.props.mediaClient.selectedVideoInputDeviceId = item.id;
    }

    handleSpeakerChange(item) {
        if (!item.id) { ; return; } // Don't do anything when query device fail (it returns an empty device with id='')
        this.props.mediaClient.selectedAudioSinkId = item.id;
        this.setState({
            speakerDevices: this.generateDropDownItem('audiooutput')
        });
    }

    handleDeviceClick(sender) {
        // Devices are initialising, prevent showing the device selector.
        // This probably never true because we disable the device buttons when it's initialising,
        // but just to be sure:
        if (this.props.mediaClient.inputDeviceManagerState == InputDeviceManagerState.Scanning) {
            return false;
        }
        // When the the devices disabled, clicking the device button won't show
        // any device to select, therefore we'll re-initialise the devices first
        if (sender == this._audioSelectButton
            && this.props.mediaClient.selectedAudioInputDeviceId == NotSelectedInputDeviceId) {
            this.setState({ isAudioLoading: true });
            this.props.mediaClient.selectedAudioInputDeviceId = null;// force re-initialisation to default device
            return false;
        }
        if (sender == this._videoSelectButton
            && this.props.mediaClient.selectedVideoInputDeviceId == NotSelectedInputDeviceId) {
            this.setState({ isVideoLoading: true });
            this.props.mediaClient.selectedVideoInputDeviceId = null; // force re-initialisation to default device
            return false;
        }
        return true;
    }

    generateDropDownItem(kind) {
        var result = [];
        var number = 1;
        var selectedDeviceId = null;
        if (kind == 'audioinput')
            selectedDeviceId = this.props.mediaClient.selectedAudioInputDeviceId;
        if (kind == 'videoinput')
            selectedDeviceId = this.props.mediaClient.selectedVideoInputDeviceId;
        if (kind == 'audiooutput') {
            selectedDeviceId = this.props.mediaClient.selectedAudioSinkId;
            if (!selectedDeviceId) {
                var tmp = this.props.mediaClient.mediaDevices.find(d => d.deviceId == 'default');
                if (tmp) {
                    selectedDeviceId = tmp.deviceId;
                }
            }
        }

        this.props.mediaClient.mediaDevices.forEach(device => {
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
                id: NotSelectedInputDeviceId,
                name: 'Don\'t Use',
                selected: selectedDeviceId == NotSelectedInputDeviceId
            });
        } else if (result.length == 1) {
            // special case for audiooutput:
            // when it fails to get a list of audiooutput devices,
            // it will returns just one empty device with deviceId is empty,
            // to avoid confusion to the users, we'll mark that device as selected,
            // and won't do anything when the user click on it (see handler above)
            result[0].selected = true;
        }
        return result;
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

    render() {
        return <div className="conference-list-view">
            <div className="heading">
                <div>#{this.props.mediaClient.conferenceSettings.roomId}</div>
            </div>
            <div className="margin-fix">
                <div className="body">
                    {this.state.users.map(user => user.devices.map(device =>
                        <ConferenceListCell
                            stream={typeof (this.state.streams[device.id]) != 'undefined' ? this.state.streams[device.id] : null}
                            key={device.id}
                            user={user}
                            self={user.username == this.props.mediaClient.conferenceSettings.username} />
                    ))}
                </div>
            </div>
            <div className="padding"></div>
            <div className="bottom-bar">
                {this.state.microphoneDevices ? <DeviceSelectButton ref={t => this._audioSelectButton = t}
                    onClick={this.handleDeviceClick}
                    isLoading={this.state.isAudioLoading}
                    icon={this.props.mediaClient.selectedAudioInputDeviceId == NotSelectedInputDeviceId ? 'microphone-slash' : 'microphone'}
                    gray={this.props.mediaClient.selectedAudioInputDeviceId == NotSelectedInputDeviceId}
                    selectItems={this.state.microphoneDevices}
                    title="Which Microphone?"
                    onItemClick={this.handleMicrophoneChange} /> : null}
                {this.state.cameraDevices ? <DeviceSelectButton ref={t => this._videoSelectButton = t}
                    onClick={this.handleDeviceClick}
                    isLoading={this.state.isVideoLoading}
                    icon={this.props.mediaClient.selectedVideoInputDeviceId == NotSelectedInputDeviceId ? 'camera' : 'camera'}
                    gray={this.props.mediaClient.selectedVideoInputDeviceId == NotSelectedInputDeviceId}
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