import EventTarget2 from "../utils/events";

class PopupManager extends EventTarget2
{
    notifyPopup(popup) {
        this.dispatchEvent(popup);
    }
}

var popupManager = new PopupManager();
export default popupManager;