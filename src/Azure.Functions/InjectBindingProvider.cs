using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.DependencyInjection;

namespace Rocket.Surgery.Azure.Functions
{
    public class InjectBindingProvider : IBindingProvider, IFunctionInvocationFilter, IFunctionExceptionFilter
    {
        private readonly IContainer _container;
        private readonly ConcurrentDictionary<Guid, ILifetimeScope> _scopes;

        public InjectBindingProvider(IContainer container)
        {
            _container = container;
            _scopes = new ConcurrentDictionary<Guid, ILifetimeScope>();
        }

        public Task<IBinding> TryCreateAsync(BindingProviderContext context)
        {
            IBinding binding = new InjectBinding(bindingContext => GetScope(bindingContext.FunctionInstanceId), context.Parameter.ParameterType);
            return Task.FromResult(binding);
        }

        public ILifetimeScope GetScope(BindingContext context)
        {
            return GetScope(context.FunctionInstanceId);
        }

        private ILifetimeScope GetScope(Guid id)
        {
            if (!_scopes.TryGetValue(id, out var scope))
            {
                scope = _container.BeginLifetimeScope(id);
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
