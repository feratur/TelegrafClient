using System.Net.Sockets;
using System.Text;
using System.Threading;
using InfluxDB.LineProtocol;
using InfluxDB.LineProtocol.Client;

namespace TelegrafClient.Influx
{
    public class MetricUdpSender : LineProtocolSocketBase
    {
        private readonly UdpClient _udpClient;
        private readonly string _udpHostName;
        private readonly int _udpPort;

        public MetricUdpSender(
            string hostname,
            int port)
        {
            _udpHostName = hostname;
            _udpPort = port;
            _udpClient = new UdpClient();
        }

        protected override LineProtocolWriteResult OnSend(
            string payload,
            Precision precision,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(payload))
                return new LineProtocolWriteResult(true, null);

            var buffer = Encoding.UTF8.GetBytes(payload);
            var len = _udpClient.Send(buffer, buffer.Length, _udpHostName, _udpPort);

            return new LineProtocolWriteResult(len == buffer.Length, null);
        }
    }
}