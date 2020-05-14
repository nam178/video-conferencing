namespace MediaServer.WebRtc.Managed
{
    public enum TransceiverReusabilityState
    {
        /// <summary>
        /// The transceiver is available for use.
        /// The sender has no track and can be safely replaced with newer track.
        /// </summary>
        Available,

        /// <summary>
        /// The transceiver's sender currently has track and cannot be reused
        /// </summary>
        Busy,

        /// <summary>
        /// The transceiver is nearly availble for re-use. 
        /// Awaiting confirmation from the client to ack that is has updated its UI accordingly.
        /// 
        /// If we re-use this transceiver at this stage, may lead to video from one person showing
        /// in the wrong slot from the client side.
        /// </summary>
        Fronzen
    }
}