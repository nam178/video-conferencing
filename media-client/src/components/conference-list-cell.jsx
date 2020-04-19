import React from 'react';

export default class ConferenceListCell extends React.PureComponent
{
    constructor() {
        super();
        this.state = { }
    }

    static getDerivedStateFromProps(props) {
        return {
            username: props.user.username,
            self: props.self,
            stream: props.stream
        };
    }

    componentDidMount() {
        this.setVideoStream();
    }

    componentDidUpdate() {
        this.setVideoStream();
    }

    setVideoStream() {
        if(this._video && this._video.srcObject != this.state.stream) {
            this._video.srcObject = this.state.stream;
        }
    }

    render() {
        return <div className={`conference-list-cell online`}>
            <div className="split-container">
                <div className="video-view-port">
                    <video autoPlay playsInline height="310" ref={t => this._video = t}></video>
                </div>
                <div className="username">{this.state.username} {this.state.self ? '(You)' : ''}</div>
            </div>
        </div>
    }
}