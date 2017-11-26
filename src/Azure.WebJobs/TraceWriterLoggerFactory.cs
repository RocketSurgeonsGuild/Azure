using Microsoft.Extensions.Logging;

namespace Rocket.Surgery.Azure.WebJobs
{
    /// <summary>
    /// A trace writer factory for those dependnecies that require a factory
    /// </summary>
    public class TraceWriterLoggerFactory : ILoggerFactory
    {
        private readonly ILogger _logger;

        /// <summary>
        ///
        /// </summary>
        /// <param name="logger"></param>
        public TraceWriterLoggerFactory(ILogger logger) { _logger = logger; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="categoryName"></param>
        /// <returns></returns>
        public ILogger CreateLogger(string categoryName)
        {
            return _logger;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="provider"></param>
        public void AddProvider(ILoggerProvider provider) { }

        /// <summary>
        ///
        /// </summary>
        public void Dispose() { }
    }
}
