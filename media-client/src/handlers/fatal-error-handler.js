import Logger from "../logging/logger";

var logger = new Logger('FatalErrorHandler');

export default class FatalErrorHandler {
    static failFast(errorMessage) {
        logger.error(errorMessage);
        // TODO: make nicer message
        alert('FATAL ERROR: ' + errorMessage);
        window.location.reload()
    }
}