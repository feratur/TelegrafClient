# TelegrafClient

This library provides necessary functionality for sending application logs and metrics to Telegraf.

Application logs are sent to the Telegraf's syslog server and metrics are sent to the Telegraf's socket via InfluxDB Line Protocol.

---

## How to use

To use the functionality, described below, import the namespace by `using Scoring.Server.TelegrafClient;`

To perform initial setup, call `TelegrafClient.Setup()` method:

```
public static TelegrafClient Setup(
            LogLevel minLogLevel = LogLevel.Information, // Minimal logging level
            string host = "localhost", // The hostname of the Telegraf server
            int metricPort = 8094, // Port for sending metrics (TCP)
            int logPort = 6514) // Port for sending logs (TCP)
```

The value, returned from the `Setup()` method, is `TelegrafClient` instance, that has fields `Metrics` (implementation of `MetricsCollector`) and `Logging` (implementation of `ILoggerProvider`).

`TelegrafClient` also has `SetStatic()` method (can be conveniently used like `TelegrafClient.Setup().SetStatic();`), that configures **static** classes `Metrics` and `Logging` - those classes provide the same functionality as `TelegrafClient` instance fields.

---

## Features

Logging and metrics client has been tested for compatibility with Telegraf's default TCP socket listener and syslog inputs.

### Logging

Telegraf's syslog input: https://github.com/influxdata/telegraf/tree/master/plugins/inputs/syslog

Syslog server address is configurable, but the default value is tcp://localhost:6514

RFC 5424 standard is utilized and messages are sent via TCP with octet counting framing method.

Default Facility is set to local0, ProcID is process ID, Application is set by CreateLogger<T>() method (sets it to class T's full name), Hostname is Environment.MachineName (Docker container ID).

1. LogCritical() is syslog severity Critical (2)
2. LogError() is syslog severity Error (3)
3. LogWarning() is syslog severity Warning (4)
4. LogInformation() is syslog severity Notice (5)
5. LogDebug() is syslog severity Informational (6)
6. LogTrace() is syslog severity Debug (7)

By default logs are sent to Telegraf starting from level Notice, but can be configured for any logging level and filtering can be performed on Telegraf's side.

The logging functionality is based on the following library: https://github.com/mguinness/syslog-framework-logging, but supports UTF8, TCP sockets and fixes syslog severity levels.

To improve compatibility with Telegraf, use `TelegrafSyslogSettings` class instead of `SyslogLoggerSettings` and `TelegrafSyslogProvider` instead of `SyslogLoggerProvider`.

### Metrics

InfluxDB metrics are supported, tested for compatibility with Telegraf's socket listener: https://github.com/influxdata/telegraf/tree/master/plugins/inputs/socket_listener

Default endpoint is tcp://localhost:8094

The metric protocol is described here: https://docs.influxdata.com/influxdb/v1.7/write_protocols/line_protocol_tutorial/

Metric sending functionality is taken from the library https://github.com/influxdata/influxdb-csharp, but supports writing to TCP stream.

To improve compatibility with Telegraf, use `TelegrafCollectorConfiguration` class instead of `CollectorConfiguration`.
