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
        }
        catch(fatalError)
        {
            // TODO
            window.alert('fatal error: ' + fatalError);
            return;
        }
        // Successed, we can show the device bar
        this.setState({ mediaDevices: this.inputDevices.mediaDevices });
        console.log(this.inputDevices.mediaDevices);
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
                <DeviceSelectButton icon="microphone" />
                <DeviceSelectButton icon="camera" />
                <DeviceSelectButton icon="volume-up" />
            </div>
            : null }
        </div>
    }
}