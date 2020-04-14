import React from 'react';

export default class ConferenceListCell extends React.Component
{
    constructor() {
        super();
        this.state = { }
    }

    static getDerivedStateFromProps(props) {
        return {
            username: props.user.username,
            isOnline: props.user.isOnline,
            self: props.self
        };
    }

    render() {
        return <div className={`conference-list-cell ${this.state.isOnline ? 'online' : 'offline'}`}>
            <div className="split-container">
                <div className="video-view-port">
                    { this.state.isOnline ? null : <i className="fas fa-skull-crossbones" /> }
                </div>
                <div className="username">{this.state.username} {this.state.self ? '(You)' : ''}</div>
            </div>
        </div>
    }
}