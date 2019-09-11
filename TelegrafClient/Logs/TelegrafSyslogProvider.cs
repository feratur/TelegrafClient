using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using TelegrafClient.Auxiliary;

namespace TelegrafClient.Logs
{
    public class TelegrafSyslogProvider : ILoggerProvider
    {
        private readonly string _hostName;
        private readonly string _procId;
        private readonly SyslogSettings _syslogSettings;
        private readonly ConcurrentDictionary<string, ILogger> _loggers;
        private readonly Syslog.Framework.Logging.TransportProtocols.IMessageSender _messageSender;

        public TelegrafSyslogProvider(
            string hostname,
            string appName,
            SyslogSettings syslogSettings,
            TransportProtocol transport,
            string server, 
            int port, 
            TcpSenderSettings senderSettings
        )
        {
            _syslogSettings = syslogSettings;
            _hostName = hostname;
            _procId = appName;
            _messageSender = MessageSenderFactory.CreateFromSettings(transport, server, port, _syslogSettings.UseOctetCounting, senderSettings);
            _loggers = new ConcurrentDictionary<string, ILogger>();
        }

        public ILogger CreateLogger(string name)
        {
            return _loggers.GetOrAdd(name, CreateLoggerInstance);
        }

        private ILogger CreateLoggerInstance(string name)
        {
            return new Syslog5424v1Logger(_procId, _hostName, _messageSender, name, _syslogSettings);
        }

        public ILogger CreateLogger<T>()
        {
            return CreateLogger(typeof(T).FullName);
        }

        public void Dispose()
        {
            _loggers.Clear();
        }
    }
}
