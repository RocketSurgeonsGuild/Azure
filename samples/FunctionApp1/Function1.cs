using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FunctionApp1;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Azure.Functions;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;

[assembly: WebJobsStartup(typeof(Startup))]

namespace FunctionApp1
{
    public class Startup : RocketSurgeryWebJobsStartup,  IServiceConvention
    {
        public Startup() : base(typeof(Startup).Assembly)
        {
        }

        public void Register(IServiceConventionContext context)
        {
            context.Services.AddTransient<HelloWorld>();
        }
    }

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

    public static class Function1
    {
        [FunctionName(nameof(HealthFunction))]
        public static async Task<string> HealthFunction(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")]
            HttpRequest req,
            HelloWorld helloWorld,
            ILogger logger, CancellationToken token)
        {
            await Task.Yield();
            logger.LogInformation($"C# Timer: {helloWorld.Value}");
            logger.LogInformation($"C# Timer trigger function executed at: {helloWorld.Date}");

            return "hello";
        }

        [FunctionName(nameof(Function12))]
        public static async Task Function12([TimerTrigger("* * * * * */10")]TimerInfo myTimer,
            HelloWorld helloWorld,
            ILogger logger, CancellationToken token)
        {
            await Task.Yield();
            logger.LogInformation($"C# Timer: {helloWorld.Value}");
            logger.LogInformation($"C# Timer trigger function executed at: {helloWorld.Date}");
        }
    }
}
