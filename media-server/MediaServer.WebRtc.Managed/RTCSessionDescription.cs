namespace MediaServer.WebRtc.Managed
{
    /// <summary>
    /// See https://developer.mozilla.org/en-US/docs/Web/API/RTCSessionDescription
    /// </summary>
    public struct RTCSessionDescription
    {
        public string Sdp { get; set; }

        public string Type { get; set; }

        public override string ToString() => $"[RTCSessionDescription {Type}, length={Sdp?.Length}]";
    }
}
