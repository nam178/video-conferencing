namespace MediaServer.WebRtc.Managed
{
    /// <summary>
    /// Also used for interops, don't modify
    /// </summary>
    public enum IceConnectionState
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
