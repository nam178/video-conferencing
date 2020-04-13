import React from 'react';
import {
    BrowserRouter as Router,
    Route,
    useParams
} from 'react-router-dom'
import WebSocketClient from './net/websocket-client.js';

export default class RoomView extends React.Component
{

    componentDidMount()
    {
        WebSocketClient.blah();
        // console.log(this.props.match.params.id);
        //console.log(env);
    }

    render()
    {
        return <div>RoomView</div>
    }
}