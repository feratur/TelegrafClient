using System;
using Syslog.Framework.Logging.TransportProtocols.Udp;
using TelegrafClient.Auxiliary;

namespace TelegrafClient.Logs
{
    internal static class MessageSenderFactory
    {
        public static Syslog.Framework.Logging.TransportProtocols.IMessageSender CreateFromSettings(TransportProtocol transport, string server, int port, bool useOctetCounting, TcpSenderSettings senderSettings)
        {
            switch (transport)
            {
                case TransportProtocol.Tcp:
                    return new TcpMessageSender(server, port, useOctetCounting,
                        senderSettings);
                case TransportProtocol.Udp:
                    return new UdpMessageSender(server, port);
                default:
                    throw new InvalidOperationException($"{nameof(transport)} '{transport}' is not recognized.");
            }
        }
    }
}
