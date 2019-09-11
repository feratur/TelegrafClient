using System;
using InfluxDB.Collector.Pipeline;

namespace TelegrafClient.Influx
{
    public class DelegateEmitter : IPointEmitter
    {
        readonly Action<PointData[]> _emitter;

        public DelegateEmitter(Action<PointData[]> emitter)
        {
            if (emitter == null) throw new ArgumentNullException(nameof(emitter));
            _emitter = emitter;
        }

        public void Emit(PointData[] points)
        {
            _emitter(points);
        }
    }
}