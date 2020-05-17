import Logger from "../logging/logger";

var logger = new Logger('FatalErrorHandler');

export default class FatalErrorHandler {
    /**
     * Should be called to stop the application from executing when an fatal error is encounted.
     * 
     * @param {String} errorMessage 
     */
    static failFast(errorMessage) {
        logger.error(errorMessage);
        // TODO: make nicer message
        alert('FATAL ERROR: ' + errorMessage);
        window.location.reload();
        throw errorMessage;
    }
}