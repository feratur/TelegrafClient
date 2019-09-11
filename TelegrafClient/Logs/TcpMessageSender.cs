using System;
using System.Text;
using TelegrafClient.Auxiliary;

namespace TelegrafClient.Logs
{
    public class TcpMessageSender : Syslog.Framework.Logging.TransportProtocols.IMessageSender
    {
        private readonly TcpSender _tcpSender;
        private readonly bool _useOctetCounting;

        public TcpMessageSender(string hostname, int port, bool useOctetCounting, TcpSenderSettings senderSettings)
        {
            _useOctetCounting = useOctetCounting;

            _tcpSender = TcpSender.Initialize(
                hostname,
                port,
                senderSettings
            );
        }

        public void SendMessageToServer(byte[] messageData)
        {
            byte[] dataToSend;

            if (_useOctetCounting)
            {
                var prefix = Encoding.UTF8.GetBytes($"{messageData.Length} ");
                dataToSend = new byte[prefix.Length + messageData.Length];

                Array.Copy(prefix, 0, dataToSend, 0, prefix.Length);
                Array.Copy(messageData, 0, dataToSend, prefix.Length, messageData.Length);
            }
            else
                dataToSend = messageData;

            _tcpSender.Send(dataToSend);
        }
    }
}
