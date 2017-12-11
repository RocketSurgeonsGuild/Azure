using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using FunctionApp1;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Rocket.Surgery.Azure.Functions;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;
using Rocket.Surgery.Extensions.Configuration;
using Rocket.Surgery.Extensions.DependencyInjection;
using Rocket.Surgery.Hosting;

[assembly: Convention(typeof(Contribution))]

namespace FunctionApp1
{
    public class HelloWorld
    {
        public static int value = 1;
        private readonly int _myValue;

        public HelloWorld()
        {
            _myValue = value++;
        }

        public DateTimeOffset Date => DateTimeOffset.Now + TimeSpan.FromMinutes(-5);
        public string Value => $"Hello World! ({_myValue})";
    }

    //class DIConfig : IFunctionConfiguration
    //{
    //    public IServiceProvider BuildServiceProvider(ExtensionConfigContext context, IServiceCollection services, ILogger logger,
    //        IHostingEnvironment environment)
    //    {
    //        var dependencyContext = DependencyContext.Load(typeof(DIConfig).Assembly);
    //        // DependencyContextLoader.Default.Load()

    //        var assemblyCandidateFinder = new DependencyContextAssemblyCandidateFinder(dependencyContext, logger);
    //        var assemblyProvider = new DependencyContextAssemblyProvider(dependencyContext, logger);
    //        var scanner = new AggregateConventionScanner(assemblyCandidateFinder);

    //        var extBuilder = new Microsoft.Extensions.Configuration.ConfigurationBuilder();
    //        var configurationBuilder = new ConfigurationBuilder(
    //            scanner,
    //            environment,
    //            new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build(),
    //            extBuilder,
    //            logger
    //        );

    //        configurationBuilder.Build();
    //        var configuration = extBuilder.Build();

    //        services
    //            .AddLogging()
    //            .Replace(ServiceDescriptor.Singleton(context.Config.LoggerFactory));

    //        var builder = new ServicesBuilder(
    //            scanner,
    //            assemblyProvider,
    //            assemblyCandidateFinder,
    //            services,
    //            configuration,
    //            environment,
    //            logger
    //        );

    //        return builder.Build();
    //    }
    //}

    class Contribution : IServiceConvention
    {
        public void Register(IServiceConventionContext context)
        {
            context.Services.AddTransient<HelloWorld>();
        }
    }

    public static class Function1
    {
        [FunctionName("Health")]
        public static void Health(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")]HttpRequest req,
            [_] HelloWorld container,
            CancellationToken token)
        {

        }

        [FunctionName("Function1")]
        public static void Run(
            [TimerTrigger("*/5 * * * * *")]TimerInfo myTimer,
            [_] HelloWorld helloWorld,
            [_] ILogger log)
        {
            log.LogInformation($"C# Timer: {helloWorld.Value}");
            log.LogInformation($"C# Timer trigger function executed at: {helloWorld.Date}");
        }
    }
}
