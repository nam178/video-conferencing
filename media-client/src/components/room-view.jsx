import React from 'react';
import MediaClient from '../net/media-client.js';
import ConferenceSettings from '../models/conference-settings.js';
import ConferenceListView from './conference-list-view.jsx';
import Lobby from './lobby.jsx';
import './room-view.css';


export default class RoomView extends React.Component {
    _mediaClient = new MediaClient();

    constructor(props) {
        super(props);
        this.handleUsernameChange = this.handleUsernameChange.bind(this);
        this.handleJoinRoomClick = this.handleJoinRoomClick.bind(this);
        this.handleUsersChanged = this.handleUsersChanged.bind(this);
        this.state = {
            isLoading: false,
            isUsernameInvalid: false,
            isJoinedRoom: false,
            username: localStorage.getItem('room_view_username') ?? '',
            fatalErrorMessage: null,
            users: null
        };
    }

    componentDidMount() {
        // Immediately join the room if previously joined
        if (localStorage.getItem('room_view_username')) {
            this.joinRoomAsync(localStorage.getItem('room_view_username'));
            this.setState({ isLoading: true });
        }

        // Watch for users changes
        this._mediaClient.addEventListener('users', this.handleUsersChanged);
    }

    componentWillUnmount() {
        this._mediaClient.removeEventListener('users', this.handleUsersChanged);
    }

    handleUsersChanged() {
        this.setState({ users: this._mediaClient.users });
    }

    handleJoinRoomClick() {
        // Validate username final time
        var username = (this.state.username ? this.state.username : '').trim();
        if (!username) {
            this.setState({ isUsernameInvalid: true });
            return;
        }

        // save the name for next time
        localStorage.setItem('room_view_username', this.state.username);

        // Join the room
        this.setState({ isLoading: true });
        this.joinRoomAsync(this.state.username);
    }

    handleUsernameChange(newUsername) {
        this.setState({ username: newUsername });

        // Validate username as the user type
        newUsername = (newUsername ? newUsername : '').trim();
        this.setState({ isUsernameInvalid: !newUsername });
    }

    async joinRoomAsync(username) {
        // Init the MediaClient with username and roomId
        var settings = new ConferenceSettings();
        settings.username = username;
        settings.roomId = this.props.match.params.id;

        try {
            await this._mediaClient.initializeAsync(settings);
            this.setState({ isJoinedRoom: true });
        }
        catch (e) {
            this.setState({ fatalErrorMessage: e });
        }
    }

    render() {
        // On fatal error, just display the error, nothing more
        if (this.state.fatalErrorMessage) {
            return <div>
                <div className="alert alert-danger m-3" role="alert">
                    <div>
                        <h5 className="mt-0">
                            Fatal Error
                    </h5>
                    </div>
                    <div className="mt-3 ml-3"><i className="fas fa-times"></i> {this.state.fatalErrorMessage}</div>
                </div>
            </div>;
        }

        // When there are users, that means we've joined the room,
        // can display the conference list view from here.
        if (this.state.isJoinedRoom) {
            return <ConferenceListView mediaClient={this._mediaClient} />
        }

        // Otherwise, by default we're not in a room,
        // therefore display the Lobby view.
        return <Lobby username={this.state.username}
            disabled={this.state.isLoading}
            isInvalid={this.state.isUsernameInvalid}
            roomId={this.props.match.params.id}
            onClick={this.handleJoinRoomClick}
            onUsernameChange={this.handleUsernameChange} />;
    }
}