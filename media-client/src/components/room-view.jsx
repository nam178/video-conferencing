import React from 'react';
import {
    BrowserRouter as Router,
    Route,
    useParams
} from 'react-router-dom'
import WebSocketClient from '../net/websocket-client.js';
import ConferenceSettings from '../models/conference-settings.js';
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
        this.state = {
            isLoading: false,
            isUsernameInvalid: false,
            username: localStorage.getItem('room_view_username') ?? '',
            fatalErrorMessage: null
        };
    }

    handleJoinRoomClick()
    {
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
        this.startWebSocketConnection();
    }

    handleUsernameChange(newUsername)
    {
        this.setState({username: newUsername});

        // Validate username as the user type
        newUsername = (newUsername ? newUsername : '').trim();
        this.setState({ isUsernameInvalid: !newUsername });
    }

    async startWebSocketConnection()
    {
        // Init the webSocketClient with username and roomId
        var settings = new ConferenceSettings();
        settings.username = this.state.username;
        settings.roomId = this.props.match.params.id;

        try
        {
            await this.webSocketClient.initializeAsync(settings);
        }
        catch(e) {
            this.setState({ fatalErrorMessage: e });
        }

        // Pass this point, initialization successed
    }

    render()
    {
        return <div className="room-view">
            { this.state.fatalErrorMessage 
                ? <div>
                    <div className="alert alert-danger m-3" role="alert">
                        <div>
                            <h5 className="mt-0">
                                Fatal Error
                            </h5>
                        </div>
                        <div className="mt-3 ml-3"><i className="fas fa-times"></i> {this.state.fatalErrorMessage}</div>
                    </div>
                  </div> 
                : <div>
                    <Lobby username={this.state.username} 
                        disabled={this.state.isLoading}
                        isInvalid={this.state.isUsernameInvalid}
                        roomId={this.props.match.params.id} 
                        onClick={this.handleJoinRoomClick}
                        onUsernameChange={this.handleUsernameChange} />
                </div> }
            
        </div>
    }
}