using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FunctionApp1;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Configuration;
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
using ConfigurationBuilder = Rocket.Surgery.Extensions.Configuration.ConfigurationBuilder;
using ExecutionContext = Microsoft.Azure.WebJobs.ExecutionContext;

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

    class Contribution : IServiceConvention
    {
        public void Register(IServiceConventionContext context)
        {
            context.Services.AddTransient<HelloWorld>();
        }
    }

    public static class Function1
    {
        [FunctionName(nameof(HealthFunction))]
        public static async Task HealthFunction(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")]
            HttpRequest req,
            [_] HelloWorld helloWorld,
            ILogger logger, ExecutionContext executionContext, CancellationToken token)
        {
            await Task.Yield();
            logger.LogInformation($"C# Timer: {helloWorld.Value}");
            logger.LogInformation($"C# Timer trigger function executed at: {helloWorld.Date}");
        }

        [FunctionName(nameof(Function12))]
        public static async Task Function12([TimerTrigger("* * * * * */10")]TimerInfo myTimer,
            [_] HelloWorld helloWorld,
            ILogger logger, ExecutionContext executionContext, CancellationToken token)
        {
            await Task.Yield();
            logger.LogInformation($"C# Timer: {helloWorld.Value}");
            logger.LogInformation($"C# Timer trigger function executed at: {helloWorld.Date}");
        }
    }
}
