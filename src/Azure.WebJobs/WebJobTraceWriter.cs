using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Rocket.Surgery.Azure.WebJobs
{
    /// <summary>
    /// Class WebJobTraceWriter.
    /// </summary>
    /// <seealso cref="Microsoft.Azure.WebJobs.Host.TraceWriter" />
    /// TODO Edit XML Comment Template for WebJobTraceWriter
    public class WebJobTraceWriter : TraceWriter
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebJobTraceWriter"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// TODO Edit XML Comment Template for #ctor
        public WebJobTraceWriter(ILogger logger) : base(TraceLevel.Verbose)
        {
            _logger = logger;
        }

        /// <summary>
        /// Writes a trace event.
        /// </summary>
        /// <param name="traceEvent">The <see cref="T:Microsoft.Azure.WebJobs.Host.TraceEvent" /> to trace.</param>
        /// TODO Edit XML Comment Template for Trace
        public override void Trace(TraceEvent traceEvent)
        {
            var logLevel = LogLevel.None;
            if (traceEvent.Level == TraceLevel.Verbose)
            {
                logLevel = LogLevel.Trace;
            }
            else if (traceEvent.Level == TraceLevel.Error)
            {
                logLevel = LogLevel.Error;
            }
            else if (traceEvent.Level == TraceLevel.Info)
            {
                logLevel = LogLevel.Information;
            }
            else if (traceEvent.Level == TraceLevel.Warning)
            {
                logLevel = LogLevel.Warning;
            }

            _logger.Log(
                logLevel,
                new EventId((int)traceEvent.Level, "WebJobs"),
                new ReadOnlyCollection<KeyValuePair<string, object>>(GetLogState(traceEvent).ToList()),
                traceEvent.Exception,
                (list, exception) => traceEvent.Message);
        }

        private static IEnumerable<KeyValuePair<string, object>> GetLogState(TraceEvent traceEvent)
        {
            yield return new KeyValuePair<string, object>(nameof(traceEvent.Timestamp), traceEvent.Timestamp);
            yield return new KeyValuePair<string, object>(nameof(traceEvent.Source), traceEvent.Source);
            foreach (var item in traceEvent.Properties)
            {
                yield return item;
            }
        }
    }
}
