import React from 'react';
import ConferenceListCell from './conference-list-cell.jsx';
import './conference-list-view.css'

export default class ConferenceListView extends React.Component
{
    constructor() {
        super();
        this.state = { users: null };
    }

    static getDerivedStateFromProps(props) {
        return {
            users: props.users.sort((x, y) => {
                // Show my self first
                if(x.username == props.conferenceSettings.username)
                    return  -1;
                if(y.username == props.conferenceSettings.username)
                    return 1;
                // Show offline people at the bottom
                if(!x.isOnline)
                    return 1;
                if(!y.isOnline)
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
                    { this.state.users.map(user => <ConferenceListCell key={user.id} user={user} self={user.username == this.props.conferenceSettings.username} /> ) }
                </div>
            </div>
            <div className="bottom-bar"></div>
        </div>
    }
}