using System.IO;
using System.Threading;
using InfluxDB.LineProtocol;
using InfluxDB.LineProtocol.Client;
using InfluxDB.LineProtocol.Payload;

namespace TelegrafClient.Influx
{
    public abstract class LineProtocolSocketBase : ILineProtocolClient
    {
        public LineProtocolWriteResult Write(LineProtocolPayload payload, CancellationToken cancellationToken = default(CancellationToken))
        {
            var stringWriter = new StringWriter();

            payload.Format(stringWriter);

            return OnSend(stringWriter.ToString(), Precision.Nanoseconds, cancellationToken);
        }

        public LineProtocolWriteResult Send(LineProtocolWriter lineProtocolWriter, CancellationToken cancellationToken = default(CancellationToken))
        {
            return OnSend(lineProtocolWriter.ToString(), lineProtocolWriter.Precision, cancellationToken);
        }

        protected abstract LineProtocolWriteResult OnSend(
            string payload,
            Precision precision,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}