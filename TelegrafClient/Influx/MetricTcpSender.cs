using System.Text;
using System.Threading;
using InfluxDB.LineProtocol;
using InfluxDB.LineProtocol.Client;
using TelegrafClient.Auxiliary;

namespace TelegrafClient.Influx
{
    public class MetricTcpSender : LineProtocolSocketBase
    {
        private readonly TcpSender _tcpSender;

        public MetricTcpSender(
            string hostname,
            int port, 
            TcpSenderSettings senderSettings)
        {
            _tcpSender = TcpSender.Initialize(
                hostname,
                port,
                senderSettings
            );
        }

        protected override LineProtocolWriteResult OnSend(
            string payload,
            Precision precision,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(payload))
                return new LineProtocolWriteResult(true, null);

            var buffer = Encoding.UTF8.GetBytes(payload);
            _tcpSender.Send(buffer);
            return new LineProtocolWriteResult(true, null);
        }
    }
}
