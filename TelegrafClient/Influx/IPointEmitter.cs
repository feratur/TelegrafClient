using InfluxDB.Collector.Pipeline;

namespace TelegrafClient.Influx
{
    public interface IPointEmitter
    {
        void Emit(PointData[] points);
    }
}