using System;
using System.Runtime.ExceptionServices;

namespace Rocket.Surgery.Azure.Functions
{
    public interface IRocketSurgeryFunctionExceptionContext : IRocketSurgeryFunctionFilterContext
    {
        Exception Exception { get; }
        ExceptionDispatchInfo ExceptionDispatchInfo { get; }
    }
}
