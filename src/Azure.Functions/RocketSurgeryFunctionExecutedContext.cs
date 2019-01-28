using System;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Extensions.Logging;

namespace Rocket.Surgery.Azure.Functions
{
    class RocketSurgeryFunctionExecutedContext : IRocketSurgeryFunctionExecutedContext
    {
        public RocketSurgeryFunctionExecutedContext(
            IServiceProvider serviceProvider,
            IReadOnlyDictionary<string, object> arguments,
            Guid functionInstanceId,
            string functionName,
            IDictionary<string, object> properties,
            ILogger logger,
            FunctionResult functionResult)
        {
            ServiceProvider = serviceProvider;
            Arguments = arguments;
            FunctionInstanceId = functionInstanceId;
            FunctionName = functionName;
            Properties = properties;
            Logger = logger;
            FunctionResult = functionResult;
        }

        public IServiceProvider ServiceProvider { get; }
        public IReadOnlyDictionary<string, object> Arguments { get; }
        public Guid FunctionInstanceId { get; }
        public string FunctionName { get; }
        public IDictionary<string, object> Properties { get; }
        public ILogger Logger { get; }
        public FunctionResult FunctionResult { get; }
    }
}
