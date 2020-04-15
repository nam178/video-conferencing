import Logger from '../logging/logger.js';

export default class InputDevices
{
    /**
     * @return {MediaStream}
     */
    get stream() { return this._stream; }

    /**
     * @returns {String}
     */
    get currentAudioInputDeviceId() {
        return this._currentAudioInputDeviceId;
    }

    /**
     * @returns {String}
     */
    get currentVideoInputDeviceId() {
        return this._currentVideoInputDeviceId;
    }

    /**
     * @return {MediaDeviceInfo[]}
     */
    get mediaDevices() { return this._mediaDevices; }

    constructor() {
        this._logger = new Logger('InputDevices');
        this._currentAudioInputDeviceId = null;
        this._currentVideoInputDeviceId = null;
    }

    initializeAsync()
    {
        return new Promise((resolve, reject) => {
            navigator.mediaDevices.getUserMedia({
                    audio: true,
                    video: true
                })
                .then((stream) => {
                    this._stream = stream;
                    this._logger.info('Media stream accquired', stream);
                    // Set the current device ids
                    stream.getTracks().forEach(track => {
                        if(track.kind == 'audio')
                            this._currentAudioInputDeviceId = track.getSettings().deviceId;
                        if(track.kind == 'video')
                            this._currentVideoInputDeviceId = track.getSettings().deviceId;
                    });
                    // Got the perms, but we'll still querying available devices
                    // so that we can let the user select
                    navigator.mediaDevices.enumerateDevices()
                        .then(devices => {
                            this._logger.info('Devices accquired', devices);
                            this._mediaDevices = devices;
                            resolve();
                        })
                        .catch((err) => {
                            this._logger.error('Failed accessing media device: ' + err);
                            reject('Failed querying devices: ' + err);
                        });
                })
                .catch((err) => {
                    this._logger.error('Failed accessing media device: ' + err);
                    reject('Failed accessing your camera/microphone, please give permission.');
                });
        });
    }
}