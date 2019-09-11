using System;
using System.Collections.Generic;
using InfluxDB.Collector;
using Microsoft.Extensions.Logging;
using TelegrafClient.Influx;
using TelegrafClient.Logs;

namespace TelegrafClient
{
    public class TelegrafClient
    {
        public readonly PipelinedMetricsCollector Metrics;
        public readonly TelegrafSyslogProvider Logging;

        private TelegrafClient(PipelinedMetricsCollector metrics, TelegrafSyslogProvider logging)
        {
            Metrics = metrics;
            Logging = logging;
        }

        public static TelegrafClient Setup() => Setup(TelegrafClientSettings.Default());

        public static TelegrafClient Setup(TelegrafClientSettings settings)
        {
            return new TelegrafClient(GetMetrics(settings), GetLogs(settings));
        }

        public static TelegrafClient SetupLogsOnly(TelegrafClientSettings settings)
        {
            return new TelegrafClient(null, GetLogs(settings));
        }

        public static TelegrafClient SetupMetricsOnly(TelegrafClientSettings settings)
        {
            return new TelegrafClient(GetMetrics(settings), null);
        }

        private static PipelinedMetricsCollector GetMetrics(TelegrafClientSettings settings)
        {
            var config = TelegrafCollectorConfiguration.Create()
                .Tag.With("hostname", settings.Hostname)
                .Tag.With("appname", settings.Appname);

            switch (settings.MessageTransportProtocol)
            {
                case TransportProtocol.Tcp:
                    return config.WriteTo.Tcp(
                            settings.ServerHost, 
                            settings.MetricServerPort,
                            settings.TcpSenderSettings)
                        .CreateCollector();
                case TransportProtocol.Udp:
                    return config.WriteTo.Udp(settings.ServerHost, settings.MetricServerPort).CreateCollector();
                default:
                    throw new ArgumentOutOfRangeException(nameof(settings.MessageTransportProtocol));
            }
        }

        private static TelegrafSyslogProvider GetLogs(TelegrafClientSettings settings)
        {
            return new TelegrafSyslogProvider(
                settings.Hostname,
                settings.Appname,
                settings.SyslogSettings,
                settings.MessageTransportProtocol,
                settings.ServerHost,
                settings.LogServerPort,
                settings.TcpSenderSettings
                );
        }

        public TelegrafClient SetStatic()
        {
            global::TelegrafClient.Logging.Provider = Logging;
            global::TelegrafClient.Metrics.Collector = Metrics;

            return this;
        }
    }

    public static class Logging
    {
        public static TelegrafSyslogProvider Provider { get; set; }

        public static ILogger CreateLogger<T>() => Provider.CreateLogger(typeof(T).FullName);
    }

    public static class Metrics
    {
        public static PipelinedMetricsCollector Collector { get; set; }

        public static void Increment(string measurement, long value = 1, IReadOnlyDictionary<string, string> tags = null)
        {
            Collector.Increment(measurement, value, tags);
        }

        public static void Measure(string measurement, object value, IReadOnlyDictionary<string, string> tags = null)
        {
            Collector.Measure(measurement, value, tags);
        }

        public static IDisposable Time(string measurement, IReadOnlyDictionary<string, string> tags = null)
        {
            return Collector.Time(measurement, tags);
        }

        public static void Write(string measurement, IReadOnlyDictionary<string, object> fields, IReadOnlyDictionary<string, string> tags = null)
        {
            Collector.Write(measurement, fields, tags);
        }

        public static CollectorConfiguration Specialize()
        {
            return Collector.Specialize();
        }

        public static void Close()
        {
            Collector.Dispose();
        }
    }
}
