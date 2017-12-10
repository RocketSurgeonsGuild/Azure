using System;
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

                context.AddBindingRule<_Attribute>().Bind(injectBindingProvider);
                context.AddBindingRule<InjectAttribute>().Bind(injectBindingProvider);
                context.AddBindingRule<ServiceAttribute>().Bind(injectBindingProvider);

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
}
