using System.Threading;
using System.Threading.Tasks;

namespace Rocket.Surgery.Azure.Functions
{
    public interface IRocketSurgeryFunctionInvocationFilter
    {
        Task OnExecutingAsync(IRocketSurgeryFunctionExecutingContext context, CancellationToken cancellationToken);
        Task OnExecutedAsync(IRocketSurgeryFunctionExecutedContext context, CancellationToken cancellationToken);
    }
}
