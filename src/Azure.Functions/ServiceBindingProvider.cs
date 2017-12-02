using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Rocket.Surgery.Azure.Functions
{
    public class ServiceBindingProvider : IBindingProvider, IFunctionInvocationFilter, IFunctionExceptionFilter
    {
        private readonly ServiceCollection _collection;
        private readonly IServiceProvider _container;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<Guid, IServiceScope> _scopes;

        public ServiceBindingProvider(
            ServiceCollection collection,
            IServiceProvider container,
            ILogger logger)
        {
            _collection = collection;
            _container = container;
            _logger = logger;
            _scopes = new ConcurrentDictionary<Guid, IServiceScope>();
        }

        public Task<IBinding> TryCreateAsync(BindingProviderContext context)
        {
            if (_collection.All(z => z.ServiceType != context.Parameter.ParameterType)
                && context.Parameter.ParameterType != typeof(ILogger)
                )
            {
                Console.Error.WriteLine($"Unable to bind service {context.Parameter.ParameterType.FullName}");
                _logger.LogInformation("Unable to bind service {Type}", context.Parameter.ParameterType.FullName);
                return null;
            }

            IBinding binding = new ServiceBinding(
                bindingContext => GetScope(bindingContext.FunctionInstanceId),
                context.Parameter.ParameterType
            );
            return Task.FromResult(binding);
        }

        public IServiceScope GetScope(BindingContext context)
        {
            return GetScope(context.FunctionInstanceId);
        }

        private IServiceScope GetScope(Guid id)
        {
            if (!_scopes.TryGetValue(id, out var scope))
            {
                scope = _container.GetRequiredService<IServiceScopeFactory>().CreateScope();
                _scopes.TryAdd(id, scope);
            }
            return scope;
        }

        private void DisposeScope(Guid id)
        {
            if (_scopes.TryRemove(id, out var scope))
            {
                scope.Dispose();
            }
        }

        Task IFunctionInvocationFilter.OnExecutingAsync(FunctionExecutingContext context, CancellationToken cancellationToken)
        {
            GetScope(context.FunctionInstanceId);
            cancellationToken.Register(() => DisposeScope(context.FunctionInstanceId));
            return Task.CompletedTask;
        }

        Task IFunctionInvocationFilter.OnExecutedAsync(FunctionExecutedContext context, CancellationToken cancellationToken)
        {
            DisposeScope(context.FunctionInstanceId);
            return Task.CompletedTask;
        }

        Task IFunctionExceptionFilter.OnExceptionAsync(FunctionExceptionContext context, CancellationToken cancellationToken)
        {
            DisposeScope(context.FunctionInstanceId);
            return Task.CompletedTask;
        }
    }
}
