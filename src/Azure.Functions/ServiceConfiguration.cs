using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;
using Rocket.Surgery.Extensions.Configuration;
using Rocket.Surgery.Extensions.DependencyInjection;
using Rocket.Surgery.Hosting;

namespace Rocket.Surgery.Azure.Functions
{
    public class ServiceConfiguration : IExtensionConfigProvider
    {
        public void Initialize(ExtensionConfigContext context)
        {
            var env = new HostingEnvironment(
                Environment.GetEnvironmentVariable("WEBSITE_SLOT_NAME") ?? string.Empty,
                Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME") ?? string.Empty,
                null,
                null
            );

            //var logger = context.Config.LoggerFactory.CreateLogger("Worker.Container");
            var logger = new TraceWriterLogger(context.Trace);

            try
            {
                var assemblyCandidateFinder = new AppDomainAssemblyCandidateFinder(logger: logger);
                var assemblyProvider = new AppDomainAssemblyProvider();
                var scanner = new AggregateConventionScanner(assemblyCandidateFinder);

                var extBuilder = new Microsoft.Extensions.Configuration.ConfigurationBuilder();
                var configurationBuilder = new ConfigurationBuilder(
                    scanner,
                    env,
                    new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build(),
                    extBuilder,
                    logger
                );

                configurationBuilder.Build();
                var configuration = extBuilder.Build();

                var services = new ServiceCollection();
                services
                    .AddLogging()
                    .Replace(ServiceDescriptor.Singleton(context.Config.LoggerFactory));

                var builder = new ServicesBuilder(
                    scanner,
                    assemblyProvider,
                    assemblyCandidateFinder,
                    services,
                    configuration,
                    env,
                    logger
                );

                var container = builder.Build();

                var injectBindingProvider = new ServiceBindingProvider(services, container, logger);
                context.Config.RegisterBindingExtension(injectBindingProvider);

                var registry = context.Config.GetService<IExtensionRegistry>();
                registry.RegisterExtension(typeof(IFunctionInvocationFilter), injectBindingProvider);
                registry.RegisterExtension(typeof(IFunctionExceptionFilter), injectBindingProvider);
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "error could not configure service");
            }
        }
    }

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
            throw new NotImplementedException();
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        internal static TraceLevel GetLogLevel(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.None:
                    return TraceLevel.Off;
                case LogLevel.Error:
                    return TraceLevel.Error;
                case LogLevel.Warning:
                    return TraceLevel.Warning;
                case LogLevel.Information:
                    return TraceLevel.Info;
                case LogLevel.Debug:
                    return TraceLevel.Verbose;
                default:
                    throw new InvalidOperationException($"'{logLevel}' is not a valid level.");
            }
        }
    }
}
