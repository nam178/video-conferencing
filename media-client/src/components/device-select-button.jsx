
import React from 'react';
import popupManager from '../models/popup-manager.js';
import './device-select-button.css';

export default class DeviceSelectButton extends React.Component
{
    constructor() {
        super();
        this.handleClick = this.handleClick.bind(this);
        this.handleItemClick = this.handleItemClick.bind(this);
        this.handleOtherPopups = this.handleOtherPopups.bind(this);
        this.state = { isOptionsShow: false };
    }

    componentDidMount() {
        popupManager.addEventListener('popup', this.handleOtherPopups);
    }

    componentWillUnmount() {
        popupManager.removeEventListener('popup', this.handleOtherPopups);
    }

    handleOtherPopups(e) {
        if(e.detail != this) {
            this.setState({ isOptionsShow: false });
        }
    }

    handleClick() {
        if(this.props.onClick && !this.props.onClick(this)) {
            return;
        }
        this.setOptionsVisibility(!this.state.isOptionsShow);
    }

    setOptionsVisibility(value) {
        this.setState({ isOptionsShow: value }, () => {
            // Need to notify the popup manager when we show a popup,
            // so that other popups will disappear
            if(this.state.isOptionsShow)
            {
                popupManager.notifyPopup(this);
            }
        });
    }

    handleItemClick(item) {
        if(this.props.onItemClick) {
            this.props.onItemClick(item);
            this.setState({ isOptionsShow: false });
        }
    }

    render() {
        return <div className={`device-select-button ${this.props.gray ? 'gray' : ''} ${this.state.isOptionsShow ? 'options-show' : ''}`}>
            { (this.props.selectItems && this.state.isOptionsShow) ? 
            <div className="select-box" style={{top: `-${(this.props.selectItems.length+1) * 45 + 15}px`}}>
                <div className="select-box-title"><span>{this.props.title}</span></div>
                <div className="select-box-body">
                    {this.props.selectItems.map(item => 
                        <div key={item.id} onClick={() => this.handleItemClick(item)} title={item.name}>
                            <i className={`${item.selected ? 'fas' : 'far'} fa-circle mr-2`}></i>
                            {item.name}
                    </div>)}
                </div>
                <div className="triangle"></div>
            </div>
            : null }
            <div className="clickable" onClick={this.handleClick}>
                { this.props.isLoading ? 
                <div className="spinner-border text-light" role="status">
                    <span className="sr-only">Loading...</span>
                </div>
                : 
                <i className={`fas fa-${this.props.icon}`}></i> }
            </div>
        </div>
    }
}