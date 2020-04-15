class PopupManager extends EventTarget
{
    notifyPopup(popup) {
        this.dispatchEvent(new CustomEvent('popup', {detail: popup}));
    }
}

var popupManager = new PopupManager();
export default popupManager;