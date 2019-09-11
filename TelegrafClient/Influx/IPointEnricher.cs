using InfluxDB.Collector.Pipeline;

namespace TelegrafClient.Influx
{
    public interface IPointEnricher
    {
        void Enrich(PointData pointData);
    }
}