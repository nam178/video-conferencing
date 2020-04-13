import React from "react";
import ReactDOM from "react-dom";
import RoomView  from './components/room-view.jsx';
import {
    BrowserRouter as Router,
    Route
} from 'react-router-dom'
import './index.css';

class App extends React.Component
{

    render()
    {
        return <div>
            <Router>
                <Route path="/rooms/:id" component={RoomView} />
            </Router>
        </div>
    }
}

ReactDOM.render(<App />, document.querySelector("#root"));