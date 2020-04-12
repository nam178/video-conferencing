namespace MediaServer.WebRtc.Managed
{
    /// <summary>
    /// See https://developer.mozilla.org/en-US/docs/Web/API/RTCPeerConnection/iceGatheringState
    /// </summary>
    public enum RTCIceGatheringState
    {
        New = 0,
        Gathering = 1,
        Complete = 2,
    };
}
