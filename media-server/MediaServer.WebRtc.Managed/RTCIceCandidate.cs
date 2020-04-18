namespace MediaServer.WebRtc.Managed
{
    /// <summary>
    /// See https://developer.mozilla.org/en-US/docs/Web/API/RTCIceCandidate
    /// </summary>
    public struct RTCIceCandidate
    {
        public string Candidate { get; set; }

        public string SdpMid { get; set; }

        public int SdpMLineIndex { get; set; }

        internal RTCIceCandidate(PeerConnectionObserverInterop.IceCandidate other)
        {
            Candidate = other.Sdp;
            SdpMid = other.SdpMid;
            SdpMLineIndex = other.MLineIndex;
        }

        public override string ToString() => $"[RTCIceCandidate length={Candidate?.Length}B]";
    }
}
