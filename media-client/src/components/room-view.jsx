import React from 'react';
import WebSocketClient from '../net/websocket-client.js';
import ConferenceSettings from '../models/conference-settings.js';
import ConferenceListView from './conference-list-view.jsx';
import Lobby from './lobby.jsx';
import './room-view.css';


export default class RoomView extends React.Component
{
    /**
     * @returns {WebSocketClient}
     */
    get webSocketClient() { return this._webSocketClient; }

    constructor(props)
    {
        super(props);
        this._webSocketClient = new WebSocketClient();
        this.handleUsernameChange = this.handleUsernameChange.bind(this);
        this.handleJoinRoomClick = this.handleJoinRoomClick.bind(this);
        this.handleUsersChanged = this.handleUsersChanged.bind(this);
        this.state = {
            isLoading: false,
            isUsernameInvalid: false,
            username: localStorage.getItem('room_view_username') ?? '',
            fatalErrorMessage: null,
            users: null
        };
    }

    componentDidMount() {
        // Immediately join the room if previously joined
        if(localStorage.getItem('room_view_username'))
        {
            this.joinRoomAsync(localStorage.getItem('room_view_username'));
            this.setState({ isLoading: true });
        }

        // Watch for users changes
        this.webSocketClient.addEventListener('users', this.handleUsersChanged);
    }

    componentWillUnmount() {
        this.webSocketClient.removeEventListener('users', this.handleUsersChanged);
    }

    handleUsersChanged() {
        this.setState({ users: this.webSocketClient.users });
    }

    handleJoinRoomClick() {
        // Validate username final time
        var username = (this.state.username ? this.state.username : '').trim();
        if(!username)
        {
            this.setState({ isUsernameInvalid: true });
            return;
        }
        
        // save the name for next time
        localStorage.setItem('room_view_username', this.state.username);

        // Begin WebSocket connection
        this.setState({isLoading: true});
        this.joinRoomAsync(this.state.username);
    }

    handleUsernameChange(newUsername) {
        this.setState({username: newUsername});

        // Validate username as the user type
        newUsername = (newUsername ? newUsername : '').trim();
        this.setState({ isUsernameInvalid: !newUsername });
    }

    async joinRoomAsync(username) {
        // Init the webSocketClient with username and roomId
        var settings = new ConferenceSettings();
        settings.username = username;
        settings.roomId = this.props.match.params.id;

        try
        {
            await this.webSocketClient.initializeAsync(settings);
        }
        catch(e) {
            this.setState({ fatalErrorMessage: e });
            return;
        }

        // Pass this point, initialization successed
        // state.users will get updated and it will trigger a re-render
    }

    render()
    {
        // On fatal error, just display the error, nothing more
        if(this.state.fatalErrorMessage)
        {
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
        if(this.state.users != null)
        {
            return <ConferenceListView users={this.state.users} conferenceSettings={this.webSocketClient.conferenceSettings} />
        }

        // Otherwise, by default we're not in a room,
        // therefore display the Lobby view.
        return  <Lobby username={this.state.username} 
            disabled={this.state.isLoading}
            isInvalid={this.state.isUsernameInvalid}
            roomId={this.props.match.params.id} 
            onClick={this.handleJoinRoomClick}
            onUsernameChange={this.handleUsernameChange} />;
    }
}