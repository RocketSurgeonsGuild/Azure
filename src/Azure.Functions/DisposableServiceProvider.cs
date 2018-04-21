using System;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ExecutionContext = Microsoft.Azure.WebJobs.ExecutionContext;

namespace Rocket.Surgery.Azure.Functions
{
    class DisposableServiceProvider : IServiceProvider, IServiceScope
    {
        private readonly IServiceScope _serviceScope;
        private readonly ILogger _logger;
        private readonly ExecutionContext _executionContext;
        private readonly object[] _additionalServices;
        private readonly ILoggerFactory _loggerFactory;

        public DisposableServiceProvider(IServiceScope serviceScope, ILogger logger, ExecutionContext executionContext, params object[] additionalServices)
        {
            _serviceScope = serviceScope;
            _logger = logger;
            _executionContext = executionContext;
            _additionalServices = additionalServices;
            _loggerFactory = new LoggerFactory(new[] { new LoggerProvider(logger) });
        }

        public void Dispose()
        {
            _serviceScope.Dispose();
        }

        public IServiceProvider ServiceProvider => this;
        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(ILoggerFactory))
                return _loggerFactory;
            if (serviceType == typeof(ILogger))
                return _logger;
            if (serviceType == typeof(ExecutionContext))
                return _executionContext;
            return _additionalServices.FirstOrDefault(x => x.GetType() == serviceType) ?? _serviceScope.ServiceProvider.GetService(serviceType);
        }
    }
}
