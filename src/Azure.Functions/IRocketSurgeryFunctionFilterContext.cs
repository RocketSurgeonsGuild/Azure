using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Rocket.Surgery.Azure.Functions
{
    public interface IRocketSurgeryFunctionFilterContext
    {
        IServiceProvider ServiceProvider { get; }
        Guid FunctionInstanceId { get; }
        string FunctionName { get; }
        IDictionary<string, object> Properties { get; }
        ILogger Logger { get; }
    }
}
