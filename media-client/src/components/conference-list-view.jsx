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
            mediaDevices: null
        };
        this._inputDevices = new InputDevices();
    }

    async componentDidMount() {
        // Query devices
        try
        {
            await this.inputDevices.initializeAsync();

            this.inputDevices.stream.getTracks().forEach(track => {
                console.info(track, track.getSettings());
            });
        }
        catch(fatalError)
        {
            // TODO
            window.alert('fatal error: ' + fatalError);
            return;
        }
        // Successed, we can show the device bar
        this.setState({ 
            mediaDevices: this.inputDevices.mediaDevices,
            microphoneDevices: this.generateDropDownItem('audioinput'),
            cameraDevices: this.generateDropDownItem('videoinput'),
            audioDevices: this.generateDropDownItem('audiooutput'),
        });
    }

    generateDropDownItem(kind) {
        var result = [];
        var number = 1;
        var selectedDeviceId = null;
        if(kind == 'audioinput')
            selectedDeviceId = this.inputDevices.currentAudioInputDeviceId;
        if(kind == 'videoinput')
            selectedDeviceId = this.inputDevices.currentVideoInputDeviceId;

        this.inputDevices.mediaDevices.forEach(device => {
            if(device.kind == kind) {
                result.push({
                    id: device.deviceId,
                    name: device.label || ConferenceListView.getDefaultName(number, kind),
                    selected: device.deviceId == selectedDeviceId
                })
                number++;
            }
        });
        result.push({id: '_disable', name: 'Don\'t Use'});
        return result;
    }

    static getDefaultName(number, kind) {
        switch(kind) {
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
            { this.state.mediaDevices ? 
            <div className="bottom-bar">
                <DeviceSelectButton icon="microphone" selectItems={this.state.microphoneDevices} title="Which Microphone?" />
                <DeviceSelectButton icon="camera"     selectItems={this.state.cameraDevices}     title="Which Camera?" />
                <DeviceSelectButton icon="volume-up"  selectItems={this.state.audioDevices}      title="Which Speaker?"/>
            </div>
            : null }
        </div>
    }
}