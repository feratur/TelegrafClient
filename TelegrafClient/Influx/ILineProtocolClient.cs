using System.Threading;
using InfluxDB.LineProtocol;
using InfluxDB.LineProtocol.Client;
using InfluxDB.LineProtocol.Payload;

namespace TelegrafClient.Influx
{
    public interface ILineProtocolClient
    {
        LineProtocolWriteResult Send(
            LineProtocolWriter lineProtocolWriter,
            CancellationToken cancellationToken = default(CancellationToken));

        LineProtocolWriteResult Write(
            LineProtocolPayload payload,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}