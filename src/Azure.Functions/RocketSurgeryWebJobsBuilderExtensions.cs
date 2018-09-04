using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Autofac;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;
using Rocket.Surgery.Extensions.Autofac;
using ConfigurationBuilder = Rocket.Surgery.Extensions.Configuration.ConfigurationBuilder;

namespace Rocket.Surgery.Azure.Functions
{
    public static class RocketSurgeryWebJobsBuilderExtensions
    {
        internal static IContainer BuildContainer(ILogger logger, ServiceCollection services, Assembly assembly, object startupInstance)
        {
            var environmentNames = new[]
            {
                Environment.GetEnvironmentVariable("WEBSITE_SLOT_NAME"),
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                "Unknown"
            };

            var applicationNames = new[]
            {
                Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME"),
                "Functions"
            };

            var envionment = new HostingEnvironment
            {
                EnvironmentName = environmentNames.First(x => !string.IsNullOrEmpty(x)),
                ApplicationName = applicationNames.First(x => !string.IsNullOrEmpty(x)),
                ContentRootPath = null,
                ContentRootFileProvider = null
            };

            var context = DependencyContext.Load(assembly);
            var assemblyCandidateFinder = new DependencyContextAssemblyCandidateFinder(context, logger);
            var assemblyProvider = new DependencyContextAssemblyProvider(context, logger);
            var scanner = new AggregateConventionScanner(assemblyCandidateFinder);

            if (startupInstance is IConvention convention)
            {
                scanner.PrependConvention(convention);
            }

            var extBuilder = new Microsoft.Extensions.Configuration.ConfigurationBuilder();
            var properties = new Dictionary<object, object>();
            var diagnosticSource = new DiagnosticListener("Rocket.Surgery.Azure");
            var configurationBuilder = new ConfigurationBuilder(
                scanner,
                envionment,
                new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build(),
                extBuilder,
                diagnosticSource,
                properties
            );

            configurationBuilder.Build();
            var configuration = extBuilder
                .AddEnvironmentVariables()
                .Build();

            services.AddLogging();

            var diBuilder = new AutofacBuilder(
                new ContainerBuilder(),
                scanner,
                assemblyProvider,
                assemblyCandidateFinder,
                services,
                configuration,
                envionment,
                diagnosticSource,
                properties
            );

            return diBuilder.Build();
        }

        public static IWebJobsBuilder AddRocketSurgery(this IWebJobsBuilder builder, Assembly  assembly, object startupInstance)
        {
            //builder.AddExtension<ServiceConfiguration>();
            var logger = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider()
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("WebJobsBuilder");

            var services = new ServiceCollection();
            foreach (var s in builder.Services.Where(x =>
                x.ServiceType == typeof(ILoggerFactory) || x.ServiceType == typeof(ILogger<>) ||
                x.ServiceType == typeof(IConfigureOptions<LoggerFilterOptions>)))
            {
                services.Add(s);
            }

            var container = BuildContainer(logger, services, assembly, startupInstance);

            var injectBindingProvider = new ServiceBindingProvider(container, logger);
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IBindingProvider>(injectBindingProvider));
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IFunctionFilter>(injectBindingProvider));

            return builder;
        }
    }
}
