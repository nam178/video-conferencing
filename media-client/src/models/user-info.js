class DeviceInfo {
    get id() { return this._id; }

    constructor(props) {
        this._id = props.deviceId;
    }
}

export default class UserInfo {
    get id() { return this._id; }

    get username() { return this._username; }

    get devices() { return this._devices; }

    constructor(props) {
        this._id = props.id;
        this._username = props.username;
        this._devices = [];
        props.devices.forEach(dProps => this._devices.push(new DeviceInfo(dProps)));
    }
}