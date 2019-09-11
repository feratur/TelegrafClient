using System;
using System.Collections.Generic;
using System.Linq;
using InfluxDB.Collector.Pipeline;
using TelegrafClient.Auxiliary;

namespace TelegrafClient.Influx
{
    public class PipelinedCollectorSocketConfiguration
    {
        readonly TelegrafCollectorConfiguration _configuration;
        readonly List<Action<PointData[]>> _emitters = new List<Action<PointData[]>>();
        private ILineProtocolClient _client;

        public PipelinedCollectorSocketConfiguration(
            TelegrafCollectorConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            _configuration = configuration;
        }

        public TelegrafCollectorConfiguration Tcp(string hostname, int port, TcpSenderSettings senderSettings)
        {
            _client = new MetricTcpSender(hostname, port, senderSettings);
            return _configuration;
        }

        public TelegrafCollectorConfiguration Udp(string hostname, int port)
        {
            _client = new MetricUdpSender(hostname, port);
            return _configuration;
        }

        public TelegrafCollectorConfiguration Emitter(Action<PointData[]> emitter)
        {
            if (emitter == null) throw new ArgumentNullException(nameof(emitter));
            _emitters.Add(emitter);
            return _configuration;
        }

        public IPointEmitter CreateEmitter(IPointEmitter parent, out Action dispose)
        {
            if (_client == null && !_emitters.Any())
            {
                dispose = null;
                return parent;
            }

            if (parent != null)
                throw new ArgumentException("Parent may not be specified here");

            var result = new List<IPointEmitter>();

            if (_client != null)
            {
                var emitter = new LineProtocolEmitter(_client);
                dispose = emitter.Dispose;
                result.Add(emitter);
            }
            else
            {
                dispose = () => { };
            }

            foreach (var emitter in _emitters)
            {
                result.Add(new DelegateEmitter(emitter));
            }

            return new AggregateEmitter(result);
        }
    }
}