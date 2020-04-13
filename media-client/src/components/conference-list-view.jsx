import React from 'react';

export default class ConferenceListView extends React.Component
{
    constructor() {
        super();
        this.state = {};
    }

    render() {
        return <div className="conference-list-view">
            <div className="heading">
                <h1>#{this.props.conferenceSettings.roomId}</h1>
            </div>
            <div className="body">
                
            </div>
        </div>
    }
}