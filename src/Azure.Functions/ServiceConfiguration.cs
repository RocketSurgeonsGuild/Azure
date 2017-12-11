﻿using System;
using System.Collections.Generic;
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
    public interface IFunctionConfiguration
    {
        IServiceProvider BuildServiceProvider(
            ExtensionConfigContext context, 
            IEnumerable<Assembly> assemblies, 
            IServiceCollection services, 
            ILogger logger, 
            IHostingEnvironment environment
        );
    }

    public class ServiceConfiguration : IExtensionConfigProvider, IFunctionConfiguration
    {
        public void Initialize(ExtensionConfigContext context)
        {
            var env = new HostingEnvironment(
                Environment.GetEnvironmentVariable("WEBSITE_SLOT_NAME") ?? string.Empty,
                Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME") ?? string.Empty,
                null,
                null
            );

            var logger = new TraceWriterLogger(context.Trace);

            try
            {
                var assemblies = context.Config.TypeLocator.GetTypes()
                    .Where(x => !x.FullName.StartsWith("Microsoft."))
                    .Where(x => !x.FullName.StartsWith("System."))
                    .Where(x => !x.FullName.StartsWith("Azure."))
                    .Select(x => x.GetTypeInfo().Assembly)
                    .Distinct()
                    .Except(new[] { typeof(ServiceConfiguration).Assembly })
                    .ToArray();

                var containerInvoker = assemblies
                    .SelectMany(x => x.GetTypes())
                    .Where(x => x.IsClass)
                    .FirstOrDefault(typeof(IFunctionConfiguration).IsAssignableFrom) ?? typeof(ServiceConfiguration);

                var services = new ServiceCollection();
                var invoker = Activator.CreateInstance(containerInvoker) as IFunctionConfiguration;
                var container = invoker.BuildServiceProvider(context, assemblies, services, logger, env);

                var injectBindingProvider = new ServiceBindingProvider(services, container, logger);

                //context.AddBindingRule<_Attribute>().Bind(injectBindingProvider);
                //context.AddBindingRule<InjectAttribute>().Bind(injectBindingProvider);
                //context.AddBindingRule<ServiceAttribute>().Bind(injectBindingProvider);
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

        public IServiceProvider BuildServiceProvider(
            ExtensionConfigContext context, 
            IEnumerable<Assembly> assemblies, 
            IServiceCollection services, 
            ILogger logger, 
            IHostingEnvironment environment
        )
        {
            try
            {
                var dependencyContext = assemblies
                    .Aggregate<Assembly, DependencyContext>(
                        null, (ctx, assembly) => ctx == null ? DependencyContext.Load(assembly) : ctx.Merge(DependencyContext.Load(assembly))
                    );

                var assemblyCandidateFinder = new DependencyContextAssemblyCandidateFinder(dependencyContext, logger);
                var assemblyProvider = new DependencyContextAssemblyProvider(dependencyContext, logger);
                var scanner = new AggregateConventionScanner(assemblyCandidateFinder);

                var extBuilder = new Microsoft.Extensions.Configuration.ConfigurationBuilder();
                var configurationBuilder = new ConfigurationBuilder(
                    scanner,
                    environment,
                    new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build(),
                    extBuilder,
                    logger
                );

                configurationBuilder.Build();
                var configuration = extBuilder.Build();

                services
                    .AddLogging()
                    .Replace(ServiceDescriptor.Singleton(context.Config.LoggerFactory));

                var builder = new ServicesBuilder(
                    scanner,
                    assemblyProvider,
                    assemblyCandidateFinder,
                    services,
                    configuration,
                    environment,
                    logger
                );

                return builder.Build();
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "error could not configure service");
                throw;
            }
        }
    }
}
