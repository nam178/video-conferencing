namespace MediaServer.WebRtc.Common
{
    /// <summary>
    /// See https://developer.mozilla.org/en-US/docs/Web/API/RTCIceCandidate
    /// </summary>
    public struct RTCIceCandidate
    {
        public string Candidate { get; set; }

        public string SdpMid { get; set; }

        public int SdpMLineIndex { get; set; }

        public override string ToString() => $"[RTCIceCandidate length={Candidate?.Length}B]";
    }
}
