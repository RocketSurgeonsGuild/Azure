using System.Collections.Generic;

namespace Rocket.Surgery.Azure.Functions
{
    public interface IRocketSurgeryFunctionInvocationContext : IRocketSurgeryFunctionFilterContext
    {
        IReadOnlyDictionary<string, object> Arguments { get; }
    }
}
