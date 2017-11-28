using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Extras.CommonServiceLocator;
using CommonServiceLocator;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging.Abstractions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;
using Rocket.Surgery.Extensions.Autofac;
using Rocket.Surgery.Extensions.Configuration;
using Rocket.Surgery.Hosting;

namespace Rocket.Surgery.Azure.Functions
{
    public class InjectConfiguration : IExtensionConfigProvider
    {
        public void Initialize(ExtensionConfigContext context)
        {
            var env = new HostingEnvironment(
                Environment.GetEnvironmentVariable("WEBSITE_SLOT_NAME") ?? string.Empty,
                Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME") ?? string.Empty,
                null,
                null
            );

            var logger = context.Config.LoggerFactory.CreateLogger("Container");
            var assemblyCandidateFinder= new AppDomainAssemblyCandidateFinder();
            var assemblyProvider = new AppDomainAssemblyProvider();
            var scanner = new AggregateConventionScanner(assemblyCandidateFinder);

            var extBuilder = new Microsoft.Extensions.Configuration.ConfigurationBuilder();
            var configurationBuilder = new ConfigurationBuilder(
                scanner,
                env,
                new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build(),
                extBuilder
            );

            configurationBuilder.Build(logger);
            var configuration = extBuilder.Build();

            var services = new ServiceCollection();
            services
                .AddLogging()
                .Replace(ServiceDescriptor.Singleton(context.Config.LoggerFactory));

            var builder = new AutofacBuilder(
                scanner,
                assemblyProvider,
                assemblyCandidateFinder,
                new ServiceCollection(),
                configuration,
                env
            );

            var container = builder.Build(new ContainerBuilder(), logger);
            var serviceLocator = new AutofacServiceLocator(container);
            ServiceLocator.SetLocatorProvider(() => serviceLocator);

            var injectBindingProvider = new InjectBindingProvider(container);
            context
                .AddBindingRule<InjectAttribute>()
                .Bind(injectBindingProvider);

            var registry = context.Config.GetService<IExtensionRegistry>();
            registry.RegisterExtension(typeof(IFunctionInvocationFilter), injectBindingProvider);
            registry.RegisterExtension(typeof(IFunctionExceptionFilter), injectBindingProvider);
        }
    }
}
