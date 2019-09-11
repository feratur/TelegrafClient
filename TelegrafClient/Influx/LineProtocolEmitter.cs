using System;
using InfluxDB.Collector.Pipeline;
using InfluxDB.LineProtocol.Payload;

namespace TelegrafClient.Influx
{
    public class LineProtocolEmitter : IDisposable, IPointEmitter
    {
        readonly ILineProtocolClient _client;

        public LineProtocolEmitter(ILineProtocolClient client)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            _client = client;
        }

        public void Dispose()
        {
            // This needs to ensure outstanding operations have completed
        }

        public void Emit(PointData[] points)
        {
            var payload = new LineProtocolPayload();

            foreach (var point in points)
            {
                payload.Add(new LineProtocolPoint(point.Measurement, point.Fields, point.Tags, point.UtcTimestamp));
            }

            var influxResult = _client.Write(payload);

            if (!influxResult.Success)
                throw new Exception(influxResult.ErrorMessage);
        }
    }
}