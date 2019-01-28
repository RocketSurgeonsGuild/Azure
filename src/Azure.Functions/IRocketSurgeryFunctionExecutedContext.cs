using Microsoft.Azure.WebJobs.Host.Executors;

namespace Rocket.Surgery.Azure.Functions
{
    public interface IRocketSurgeryFunctionExecutedContext : IRocketSurgeryFunctionInvocationContext
    {
        FunctionResult FunctionResult { get; }
    }
}
