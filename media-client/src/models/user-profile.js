export default class UserProfile
{
    get id () { return this._id; }
    set id(value) { this._id = value; }

    get username()  { return this._username; }
    set username(value) { this._username = value; }

    get isOnline() { return this._isOnline; }
    set isOnline(value) { this._isOnline = value; }

    constructor(props)
    {
        this.id = props.id;
        this.username = props.username;
        this.isOnline = props.isOnline;
    }
}