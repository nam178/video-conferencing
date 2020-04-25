import Logger from '../logging/logger.js';

export default class InputDeviceManager extends EventTarget {
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
     * @return {String}
     */
    get currentOutAudioSinkId() {
        return this._currentOutAudioSinkId;
    }

    set currentOutAudioSinkId(value) {
        this._currentOutAudioSinkId = value;
    }

    /**
     * @return {MediaDeviceInfo[]}
     */
    get mediaDevices() { return this._mediaDevices; }

    constructor() {
        super();
        this._mediaDevices = [];
        this._logger = new Logger('InputDeviceManager');
        this._currentAudioInputDeviceId = null;
        this._currentVideoInputDeviceId = null;
        this._currentOutAudioSinkId = null;
    }

    static NotSelectedDeviceId() { return -1; }

    /**
     * Designed so can be called multiple items to re-initialise;
     * Must be called at least once before using;
     */
    refreshAsync() {
        // Stop any running track
        if (this.stream) {
            this.stream.getTracks().forEach(track => track.stop());
            this._stream = null;
            this._triggerStreamChangeEvent();
        }

        // Query devices with callback
        var constraints = {};

        if (this.currentAudioInputDeviceId != InputDeviceManager.NotSelectedDeviceId()) {
            constraints.audio = {
                deviceId: this._currentAudioInputDeviceId ? { ideal: this._currentAudioInputDeviceId } : undefined
            };
        }

        if (this.currentVideoInputDeviceId != InputDeviceManager.NotSelectedDeviceId()) {
            constraints.video = {
                deviceId: this._currentVideoInputDeviceId ? { ideal: this._currentVideoInputDeviceId } : undefined,
                width: { ideal: 720 },
                height: { ideal: 480 },
            }
        }

        // Not requesting anything?
        if (this.currentAudioInputDeviceId == InputDeviceManager.NotSelectedDeviceId()
            && this.currentVideoInputDeviceId == InputDeviceManager.NotSelectedDeviceId()) {
            // Just refresh the device only, 
            // Note that the device list is best-effort based
            return new Promise((resolve, reject) => {
                this._refreshDevicesAsync().then(() => resolve()).catch(err => reject(err));
            });
        }

        this._logger.info('Requesting devices..', constraints);

        return new Promise((resolve, reject) => {
            navigator.mediaDevices
                .getUserMedia(constraints)
                .then((stream) => {
                    this._stream = stream;
                    this._triggerStreamChangeEvent();
                    this._logger.info('Media stream accquired', stream);
                    // Set the current device ids
                    stream.getTracks().forEach(track => {
                        if (track.kind == 'audio')
                            this._currentAudioInputDeviceId = track.getSettings().deviceId;
                        if (track.kind == 'video')
                            this._currentVideoInputDeviceId = track.getSettings().deviceId;
                        this._logger.info('Found track', track);
                    });
                    // Then refresh the devices
                    this._refreshDevicesAsync().then(() => resolve()).catch(err => reject(err));
                })
                .catch((err) => {
                    // todo: handle those
                    // AbortError
                    // NotAllowedError
                    // NotReadableError
                    // NotFoundError
                    // OverconstrainedError
                    // SecurityError
                    // TypeError
                    this._logger.error('Failed accessing media device: ' + err);
                    reject('Failed accessing your camera/microphone, please give permission.');
                });
        });
    }

    _triggerStreamChangeEvent() {
        this.dispatchEvent(new CustomEvent('streamchange'));
    }

    _refreshDevicesAsync() {
        return new Promise((resolve, reject) => {
            // Got the perms, but we'll still querying available devices
            // so that we can let the user select
            navigator.mediaDevices.enumerateDevices()
                .then(devices => {
                    this._logger.info('Devices accquired', devices);
                    this._mediaDevices = devices;
                    // Now, if the selected sink doesn't match any if the device, reset to null
                    if (!this._mediaDevices.find(device => device.kind == 'audiooutput'
                        && device.deviceId == this._currentOutAudioSinkId
                        && this._currentOutAudioSinkId)) {
                        this._logger.warn(`Could not find sink ${this._currentOutAudioSinkId}, using the default sink`);
                        this._currentOutAudioSinkId = null;
                    }
                    resolve();
                })
                .catch((err) => {
                    this._logger.error('Failed accessing media device: ' + err);
                    reject('Failed querying devices: ' + err);
                });
        });
    }
}