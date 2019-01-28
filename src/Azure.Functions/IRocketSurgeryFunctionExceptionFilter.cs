using System.Threading;
using System.Threading.Tasks;

namespace Rocket.Surgery.Azure.Functions
{
    public interface IRocketSurgeryFunctionExceptionFilter
    {
        Task OnExceptionAsync(IRocketSurgeryFunctionExceptionContext context, CancellationToken cancellationToken);
    }
}
