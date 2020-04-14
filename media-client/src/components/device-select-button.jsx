
import React from 'react';
import './device-select-button.css';

export default class DeviceSelectButton extends React.Component
{
    render() {
        return <div className="device-select-button">
            <div className="clickable">
                <i className={`fas fa-${this.props.icon}`}></i>
            </div>
        </div>
    }
}