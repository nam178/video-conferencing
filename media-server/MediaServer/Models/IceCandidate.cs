namespace MediaServer.Models
{
    public struct IceCandidate
    {
        public string SdpMid { get; set; }

        public int SdpMLineIndex { get; set; }

        public string Candidate { get; set; }
    }
}
