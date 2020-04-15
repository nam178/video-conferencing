import Logger from '../logging/logger.js';

export default class InputDevices {
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

    set currentAudioInputDeviceId(value) {
        this._currentAudioInputDeviceId = value;
    }

    /**
     * @returns {String}
     */
    get currentVideoInputDeviceId() {
        return this._currentVideoInputDeviceId;
    }

    set currentVideoInputDeviceId(value) {
        this._currentVideoInputDeviceId = value;
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

    static NotSelectedDeviceId() { return -1; }

    // Designed so can be called multiple items to re-initialise
    initializeAsync() {
        // Stop any running track
        if(this.stream)
        {
            this.stream.getTracks().forEach(track => track.stop());
            this._stream = null;
        }
        
        // Query devices with callback
        var constraints = {};

        if (this.currentAudioInputDeviceId != InputDevices.NotSelectedDeviceId()) {
            constraints.audio = {
                deviceId: this._currentAudioInputDeviceId ? { ideal: this._currentAudioInputDeviceId } : undefined
            };
        }

        if (this.currentVideoInputDeviceId != InputDevices.NotSelectedDeviceId()) {
            constraints.video = {
                deviceId: this._currentVideoInputDeviceId ? { ideal: this._currentVideoInputDeviceId } : undefined,
                width: { ideal:1280 },
                height: { ideal: 720 },
            }
        }

        // Not requesting anything?
        if(this.currentAudioInputDeviceId == InputDevices.NotSelectedDeviceId()
            && this.currentVideoInputDeviceId == InputDevices.NotSelectedDeviceId())
        {
            this._mediaDevices = [];
            return;
        }

        this._logger.info('Requesting devices..', constraints);

        return new Promise((resolve, reject) => {
            navigator.mediaDevices
                .getUserMedia(constraints)
                .then((stream) => {
                    this._stream = stream;
                    this._logger.info('Media stream accquired', stream);
                    // Set the current device ids
                    stream.getTracks().forEach(track => {
                        if (track.kind == 'audio')
                            this._currentAudioInputDeviceId = track.getSettings().deviceId;
                        if (track.kind == 'video')
                            this._currentVideoInputDeviceId = track.getSettings().deviceId;
                        this._logger.info('Found track', track);
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