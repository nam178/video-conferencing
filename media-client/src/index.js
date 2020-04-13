import React from "react";
import ReactDOM from "react-dom";
import RoomView  from './room-view.jsx';
import {
    BrowserRouter as Router,
    Route
} from 'react-router-dom'

class App extends React.Component
{

    render()
    {
        return <div>
            <Router>
                <Route path="/rooms/:id" component={RoomView}>
                </Route>
            </Router>
        </div>
    }
}

ReactDOM.render(<App />, document.querySelector("#root"));