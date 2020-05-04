export default class FatalErrorHandler {
    static handle(errorMessage) {
        // TODO: make nicer message
        alert('FATAL ERROR: ' + errorMessage);
        window.location.reload()
    }
}