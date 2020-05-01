namespace MediaServer.Api.WebSocket
{
    sealed class CommandFormat
    {
        /// <summary>
        /// Type of the command 
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// The arguments for this command
        /// </summary>
        public object Args { get; set; }
    }
}
