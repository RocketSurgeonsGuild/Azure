using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;
using Rocket.Surgery.Extensions.DependencyInjection;
using ConfigurationBuilder = Rocket.Surgery.Extensions.Configuration.ConfigurationBuilder;

namespace Rocket.Surgery.Azure.Functions
{
    public class ServiceConfiguration : IExtensionConfigProvider
    {
        public void Initialize(ExtensionConfigContext context)
        {
            var logger = context.Config.LoggerFactory.CreateLogger("ServiceConfiguration");
            var container = RocketSurgeryWebJobsBuilderExtensions.BuildContainer(logger, new ServiceCollection());

            var injectBindingProvider = new ServiceBindingProvider(container, logger);
            context.Config.RegisterBindingExtension(injectBindingProvider);

            var registry = context.Config.GetService<IExtensionRegistry>();
            registry.RegisterExtension(typeof(IFunctionInvocationFilter), injectBindingProvider);
            registry.RegisterExtension(typeof(IFunctionExceptionFilter), injectBindingProvider);
        }
    }
}
