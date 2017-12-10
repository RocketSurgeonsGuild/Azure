using System;
using System.IO;
using System.Reflection;
using FunctionApp1;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Rocket.Surgery.Azure.Functions;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Extensions.DependencyInjection;

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
        [FunctionName("Health")]
        public static void Health(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")]HttpRequest req,
            [Service] HelloWorld container)
        {

        }

        [FunctionName("Function1")]
        public static void Run(
            [TimerTrigger("*/5 * * * * *")]TimerInfo myTimer,
            [Service]             HelloWorld helloWorld,
            ILogger log)
        {
            log.LogInformation($"C# Timer: {helloWorld.Value}");
            log.LogInformation($"C# Timer trigger function executed at: {helloWorld.Date}");
        }
    }
}
