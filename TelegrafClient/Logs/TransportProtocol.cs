namespace TelegrafClient.Logs
{
    /// <summary>
    /// Available built-in transport protocols used for sending logs to a syslog server.
    /// </summary>
    public enum TransportProtocol
    {
        /// <summary>
        /// Sends the logs using UDP datagrams.
        /// </summary>
        Udp,

        /// <summary>
        /// Sends the logs using TCP socket streams.
        /// </summary>
        Tcp
    }
}
