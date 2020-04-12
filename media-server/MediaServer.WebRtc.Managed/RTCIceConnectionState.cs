namespace MediaServer.WebRtc.Managed
{
    /// <summary>
    /// See https://developer.mozilla.org/en-US/docs/Web/API/RTCPeerConnection/iceConnectionState
    /// </summary>
    public enum RTCIceConnectionState
    {
        New = 0,
        Checking = 1,
        Connected = 2,
        Completed = 3,
        Failed = 4,
        Disconnected = 5,
        Closed = 6,
        Max = 7
    };
}
