
import React from 'react';
import './device-select-button.css';

export default class DeviceSelectButton extends React.Component
{
    constructor() {
        super();
        this.handleClick = this.handleClick.bind(this);
        this.handleItemClick = this.handleItemClick.bind(this);
        this.state = { isOptionsShow: false };
    }

    handleClick() {
        this.setState({ isOptionsShow: !this.state.isOptionsShow });
    }

    handleItemClick(item) {
        if(this.props.onItemClick)
        {
            this.props.onItemClick(item);
        }
    }

    render() {
        return <div className={`device-select-button ${this.state.isOptionsShow ? 'options-show' : ''}`}>
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
                <i className={`fas fa-${this.props.icon}`}></i>
            </div>
        </div>
    }
}