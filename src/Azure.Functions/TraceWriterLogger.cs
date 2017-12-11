using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Rocket.Surgery.Azure.Functions
{
    class TraceWriterLogger : ILogger
    {
        private readonly TraceWriter _writer;

        public TraceWriterLogger(TraceWriter writer)
        {
            _writer = writer;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var @event = new TraceEvent(GetLogLevel(logLevel), formatter(state, exception), null, exception);
            if (state is IEnumerable<KeyValuePair<string, object>> objectValues)
            {
                foreach (var value in objectValues)
                {
                    @event.Properties.Add(value.Key, value.Value);
                }
            }
            else if (state is IEnumerable<KeyValuePair<string, string>> stringValues)
            {
                foreach (var value in stringValues)
                {
                    @event.Properties.Add(value.Key, value.Value);
                }
            }

            _writer.Trace(@event);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        class Disposable : IDisposable {
            public void Dispose(){}
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return new Disposable();
        }

        internal static TraceLevel GetLogLevel(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.None:
                    return TraceLevel.Off;
                case LogLevel.Error:
                case LogLevel.Critical:
                    return TraceLevel.Error;
                case LogLevel.Warning:
                    return TraceLevel.Warning;
                case LogLevel.Information:
                    return TraceLevel.Info;
                case LogLevel.Debug:
                case LogLevel.Trace:
                    return TraceLevel.Verbose;
                default:
                    throw new InvalidOperationException($"'{logLevel}' is not a valid level.");
            }
        }
    }
}
