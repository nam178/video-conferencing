import React from 'react'
import './lobby.css'

export default class Lobby extends React.Component
{
    constructor()
    {
        super();
        this.state = { };
        this.handleNameInputChange = this.handleNameInputChange.bind(this);
        this.handleClick = this.handleClick.bind(this);
    }

    handleClick()
    {
        if(this.props.onClick)
        {
            this.props.onClick();
        }
    }

    handleNameInputChange(value)
    {
        if(this.props.onUsernameChange)
        {
            this.props.onUsernameChange(value.target.value);
        }
    }

    render()
    {
        return <div className="lobby">
            <div className="lobby-card">
                <div className="card" style={{width: '18rem'}}>
                    <div className="card-body">
                        <h5 className="card-title">#{this.props.roomId}</h5>
                        <p className="font-weight-light">Please enter your name below to join this room:</p>
                        <div className="form-group">
                            <input 
                                placeholder="Funny Bunny"
                                autoFocus
                                type="ematextil" disabled={this.props.disabled}
                                className={`form-control ${this.props.isInvalid ? 'is-invalid' : ''}`} 
                                value={this.props.username} 
                                onChange={this.handleNameInputChange}  />
                        </div>
                        <div className="text-right">
                            { this.props.disabled 
                                ?  <span className="text-primary spinner-grow spinner-grow-sm mr-2" role="status" aria-hidden="true"></span>
                                : null
                            }
                            <button className="btn btn-primary" disabled={this.props.disabled} type="submit" onClick={this.handleClick}>
                                Join Room
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    }
}