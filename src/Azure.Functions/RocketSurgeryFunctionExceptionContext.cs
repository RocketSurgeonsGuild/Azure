using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using Microsoft.Extensions.Logging;

namespace Rocket.Surgery.Azure.Functions
{
    class RocketSurgeryFunctionExceptionContext : IRocketSurgeryFunctionExceptionContext
    {
        public RocketSurgeryFunctionExceptionContext(
            IServiceProvider serviceProvider,
            Guid functionInstanceId,
            string functionName,
            IDictionary<string, object> properties,
            ILogger logger,
            Exception exception,
            ExceptionDispatchInfo exceptionDispatchInfo)
        {
            ServiceProvider = serviceProvider;
            FunctionInstanceId = functionInstanceId;
            FunctionName = functionName;
            Properties = properties;
            Logger = logger;
            Exception = exception;
            ExceptionDispatchInfo = exceptionDispatchInfo;
        }

        public IServiceProvider ServiceProvider { get; }
        public Guid FunctionInstanceId { get; }
        public string FunctionName { get; }
        public IDictionary<string, object> Properties { get; }
        public ILogger Logger { get; }
        public Exception Exception { get; }
        public ExceptionDispatchInfo ExceptionDispatchInfo { get; }
    }
}
