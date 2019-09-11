using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;

namespace TelegrafClient.Logs
{
    public abstract class TelegrafSyslogLogger : ILogger
    {
        private readonly string _name;
        private readonly string _host;
        private readonly SyslogSettings _settings;
        private readonly string _processId;
        private readonly Syslog.Framework.Logging.TransportProtocols.IMessageSender _messageSender;

        protected TelegrafSyslogLogger(string name, string host, Syslog.Framework.Logging.TransportProtocols.IMessageSender messageSender, string procId, SyslogSettings syslogSettings)
        {
            if (!string.IsNullOrWhiteSpace(name) && !IsValidPrintAscii(name, ' '))
                throw new ArgumentException("Invalid parameter value", nameof(name));
            if (!string.IsNullOrWhiteSpace(host) && !IsValidPrintAscii(host, ' '))
                throw new ArgumentException("Invalid parameter value", nameof(host));
            if (!string.IsNullOrWhiteSpace(procId) && !IsValidPrintAscii(procId, ' '))
                throw new ArgumentException("Invalid parameter value", nameof(procId));

            _settings = syslogSettings;
            _name = name;
            _host = host;
            _messageSender = messageSender;
            _processId = procId ?? GetProcId()?.ToString();
        }

        protected static bool IsValidPrintAscii(string name, params char[] invalid)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            foreach (var ch in name)
            {
                if (ch < 33)
                    return false;
                if (ch > 126)
                    return false;
                if (invalid.Contains(ch))
                    return false;
            }

            return true;
        }


        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None && logLevel >= _settings.MinLogLevel;
        }

        private static readonly Dictionary<Type, char> SdValueTypes = new Dictionary<Type, char>
        {
            { typeof(sbyte), 'i' },
            { typeof(short), 'i' },
            { typeof(int), 'i' },
            { typeof(long), 'i' },
            { typeof(byte), 'u' },
            { typeof(ushort), 'u' },
            { typeof(uint), 'u' },
            { typeof(ulong), 'u' },
            { typeof(float), 'f' },
            { typeof(double), 'f' },
            { typeof(bool), 'b' }
        };

        public virtual void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));

            if (!IsEnabled(logLevel))
                return;

            string message = formatter(state, exception);

            if (String.IsNullOrEmpty(message))
                return;

            // Defined in RFC 5424, section 6.2.1, and RFC 3164, section 4.1.1.
            // If a different value is needed, then this code should probably move into the specific loggers.
            var severity = MapToSeverityType(logLevel);
            var priority = ((int)_settings.FacilityType * 8) + severity;
            var now = _settings.UseUtc ? DateTime.UtcNow : DateTime.Now;

            Dictionary<string, string> props = null;

            var stateValues = state as FormattedLogValues;

            if (stateValues != null && stateValues.Count > 1)
            {
                props = new Dictionary<string, string>(stateValues.Count - 1);

                for (var i = 0; i < stateValues.Count - 1; i++)
                {
                    var stateValue = stateValues[i];

                    if (!SdValueTypes.TryGetValue(stateValue.Value.GetType(), out var prefix))
                        prefix = 's';

                    props.Add($"{prefix}_{stateValue.Key}", stateValue.Value.ToString());
                }
            }

            var msg = FormatMessage(priority, now, _host, _name, _processId, eventId.Id, message, props);

            var raw = Encoding.UTF8.GetBytes(msg + '\n');

            try
            {
                _messageSender.SendMessageToServer(raw);
            }
            catch (Exception ex)
            {
                // Do not rethrow exception. Crashing an application just because logging has failed due to a transient unavailability of syslog server
                // does not look like a good practice.
                Console.Error.WriteLine("Logging failed: " + ex);
            }
        }

        protected abstract string FormatMessage(
            int priority, 
            DateTime now, 
            string host, 
            string name, 
            string procid, 
            int msgid, 
            string message,
            Dictionary<string, string> props);

        private static int? GetProcId()
        {
            try
            {
                // Attempt to get the process ID. This might not work on all platforms.
                return Process.GetCurrentProcess().Id;
            }
            catch
            {
                return null;
            }
        }

        internal virtual int MapToSeverityType(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                    return 7; // Syslog severity level Debug
                case LogLevel.Debug:
                    return 6; // Syslog severity level Informational
                case LogLevel.Information:
                    return 5; // Syslog severity level Notice
                case LogLevel.Warning:
                    return 4; // Syslog severity level Warning
                case LogLevel.Error:
                    return 3; // Syslog severity level Error
                case LogLevel.Critical:
                    return 2; // Syslog severity level Critical
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel));
            }
        }
    }

    /// <summary>
    /// Based on RFC 3164: https://tools.ietf.org/html/rfc3164
    /// </summary>
    public class Syslog3164Logger : TelegrafSyslogLogger
    {
        public Syslog3164Logger(string name, string host, Syslog.Framework.Logging.TransportProtocols.IMessageSender messageSender, string procId, SyslogSettings syslogSettings)
            : base(name, host, messageSender, procId, syslogSettings)
        {
        }

        protected override string FormatMessage(
            int priority,
            DateTime now,
            string host,
            string name,
            string procid,
            int msgid,
            string message,
            Dictionary<string, string> props)
        {
            var tag = name.Replace(".", String.Empty).Replace("_", String.Empty); // Alphanumeric
            tag = tag.Substring(0, Math.Min(32, tag.Length)); // Max length is 32 according to spec
            return $"<{priority}>{now:MMM dd HH:mm:ss} {host} {tag} {message}";
        }
    }

    /// <summary>
    /// Based on RFC 5424: https://tools.ietf.org/html/rfc5424
    /// </summary>
    public class Syslog5424v1Logger : TelegrafSyslogLogger
    {
        private const string NilValue = "-";
        private readonly string _structuredData;


        public Syslog5424v1Logger(string name, string host, Syslog.Framework.Logging.TransportProtocols.IMessageSender messageSender, string procId, SyslogSettings syslogSettings)
            : base(name, host, messageSender, procId, syslogSettings)
        {
            _structuredData = FormatStructuredData(syslogSettings.StructuredData);
        }

        private static string FormatStructuredData(IEnumerable<SyslogStructuredData> sd)
        {
            if (sd == null)
                return null;

            var structuredData = sd.ToList();

            if (!structuredData.Any())
                return null;

            var sb = new StringBuilder();

            foreach (var data in structuredData)
            {
                if (!IsValidPrintAscii(data.Id, '=', ' ', ']', '"'))
                    throw new InvalidOperationException($"ID for structured data {data.Id} is not valid. US Ascii 33-126 only, except '=', ' ', ']', '\"'");

                sb.Append($"[{data.Id}");

                if (data.Elements != null)
                {
                    foreach (var element in data.Elements)
                    {
                        if (!IsValidPrintAscii(element.Name, '=', ' ', ']', '"'))
                            throw new InvalidOperationException($"Element {element.Name} in structured data {data.Id} is not valid. US Ascii 33-126 only, except '=', ' ', ']', '\"'");

                        // According to spec, need to escape these characters.
                        var val = element.Value
                            .Replace("\\", "\\\\")
                            .Replace("\"", "\\\"")
                            .Replace("]", "\\]");
                        sb.Append($" {element.Name}=\"{val}\"");
                    }
                }

                sb.Append("]");
            }

            return sb.ToString();
        }

        private static string GetSdFromDict(Dictionary<string, string> kvps, string sdId)
        {
            if (kvps == null || kvps.Count == 0)
                return null;

            var sb = new StringBuilder();

            if (!IsValidPrintAscii(sdId, '=', ' ', ']', '"'))
                throw new InvalidOperationException(
                    $"ID for structured data {sdId} is not valid. US Ascii 33-126 only, except '=', ' ', ']', '\"'");

            sb.Append($"[{sdId}");

            foreach (var kvp in kvps)
            {
                if (!IsValidPrintAscii(kvp.Key, '=', ' ', ']', '"'))
                    throw new InvalidOperationException(
                        $"Element {kvp.Key} in structured data {sdId} is not valid. US Ascii 33-126 only, except '=', ' ', ']', '\"'");

                // According to spec, need to escape these characters.
                var val = kvp.Value
                    .Replace("\\", "\\\\")
                    .Replace("\"", "\\\"")
                    .Replace("]", "\\]");
                sb.Append($" {kvp.Key}=\"{val}\"");
            }

            sb.Append("]");

            return sb.ToString();
        }

        protected override string FormatMessage(
            int priority,
            DateTime now,
            string host,
            string name,
            string procid,
            int msgid,
            string message,
            Dictionary<string, string> props)
        {
            var formattedTimestamp = FormatTimestamp(now);
            var structuredData = (_structuredData ?? string.Empty) + (GetSdFromDict(props, "sd") ?? string.Empty);
            return $"<{priority}>1 {formattedTimestamp} {TrimString(host, 255) ?? NilValue} {TrimString(name, 48) ?? NilValue} {TrimString(procid, 128) ?? NilValue} {msgid} {(string.IsNullOrEmpty(structuredData) ? NilValue : structuredData)} {message}";
        }

        private static string TrimString(string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return value.Length > maxLength ? value.Substring(0, maxLength) : value;
        }

        /// <summary>
        /// Formats the date as required by RFC 5424.
        /// </summary>
        private static string FormatTimestamp(DateTime time)
        {
            return time.ToString("yyyy-MM-ddTHH:mm:ss.ffffffK", CultureInfo.InvariantCulture);
        }
    }
}
