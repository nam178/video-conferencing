namespace MediaServer.WebRtc.Managed
{
    /// <summary>
    /// Also used for interops, don't modify
    /// </summary>
    public enum IceConnectionState
    {
        IceConnectionNew = 0,
        IceConnectionChecking = 1,
        IceConnectionConnected = 2,
        IceConnectionCompleted = 3,
        IceConnectionFailed = 4,
        IceConnectionDisconnected = 5,
        IceConnectionClosed = 6,
        IceConnectionMax = 7
    };
}
