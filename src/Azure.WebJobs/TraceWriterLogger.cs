using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Rocket.Surgery.Azure.WebJobs
{
    /// <summary>
    /// A default trace writer logger
    /// </summary>
    public class TraceWriterLogger : ILogger
    {
        private readonly TraceWriter _writer;
        private readonly List<KeyValuePair<string, string>[]> _scopes = new List<KeyValuePair<string, string>[]>();

        /// <summary>
        /// Accepts a trace writer and marshals log calls over to it.
        /// </summary>
        /// <param name="writer"></param>
        public TraceWriterLogger(TraceWriter writer)
        {
            _writer = writer;
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="logLevel"></param>
        /// <param name="eventId"></param>
        /// <param name="state"></param>
        /// <param name="exception"></param>
        /// <param name="formatter"></param>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            TraceLevel level = TraceLevel.Info;
            switch (logLevel)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                    level = TraceLevel.Verbose;
                    break;
                case LogLevel.Critical:
                case LogLevel.Error:
                    level = TraceLevel.Error;
                    break;
                case LogLevel.Information:
                    level = TraceLevel.Info;
                    break;
                case LogLevel.None:
                    level = TraceLevel.Off;
                    break;
                case LogLevel.Warning:
                    level = TraceLevel.Warning;
                    break;
            }

            var @event = new TraceEvent(level, formatter(state, exception), null, exception);
            if (state is IEnumerable<KeyValuePair<string, string>> values)
            {
                foreach (var item in values)
                {
                    @event.Properties.Add(item.Key, item);
                }
            }
            foreach (var item in _scopes.SelectMany(x => x)
                .GroupBy(x => x.Key))
            {
                @event.Properties.Add(item.Key, item.LastOrDefault());
            }

            _writer.Trace(@event);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="logLevel"></param>
        /// <returns></returns>
        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="state"></param>
        /// <returns></returns>
        public IDisposable BeginScope<TState>(TState state)
        {
            if (!(state is IEnumerable<KeyValuePair<string, string>> values))
                return new Disposable(() => { });
            var result = values.ToArray();
            _scopes.Add(result);
            return new Disposable(() => _scopes.Remove(result));
        }

        class Disposable : IDisposable
        {
            private readonly Action _action;

            public Disposable(Action action)
            {
                _action = action;
            }

            public void Dispose()
            {
                _action();
            }
        }
    }

    /// <summary>
    /// A generic trace writer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TraceWriterLogger<T> : TraceWriterLogger, ILogger<T>
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="writer"></param>
        public TraceWriterLogger(TraceWriter writer) : base(writer)
        {
        }
    }
}
