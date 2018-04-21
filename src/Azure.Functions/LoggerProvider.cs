using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;

namespace Rocket.Surgery.Azure.Functions
{
    class LoggerProvider : ILoggerProvider
    {
        private readonly ILogger _logger;

        public LoggerProvider(ILogger logger)
        {
            _logger = logger;
        }

        public void Dispose() { }

        public ILogger CreateLogger(string categoryName)
        {
            return _logger;
        }
    }
}
