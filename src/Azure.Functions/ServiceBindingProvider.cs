using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Rocket.Surgery.Azure.Functions
{
    public class ServiceBindingProvider : IBindingProvider, IFunctionInvocationFilter, IFunctionExceptionFilter
    {
        private readonly IComponentContext _container;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<Guid, IServiceScope> _scopes;
        private readonly IServiceProvider _serviceProvider;

        public ServiceBindingProvider(
            IComponentContext container,
            ILogger logger)
        {
            _container = container;
            _serviceProvider = container.Resolve<IServiceProvider>();
            _logger = logger;
            _scopes = new ConcurrentDictionary<Guid, IServiceScope>();
        }

        public Task<IBinding> TryCreateAsync(BindingProviderContext context)
        {
            if (context.Parameter.GetCustomAttributes()
                .Any(z => z.GetType().GetCustomAttributes().Any(x => x is BindingAttribute)))
            {
                return Task.FromResult<IBinding>(null);
            }

            if (!_container.IsRegistered(context.Parameter.ParameterType))
            {
                return Task.FromResult<IBinding>(null);
            }

            return Task.FromResult(CreateBinding(context));
        }

        private IBinding CreateBinding(BindingProviderContext context) =>
            new ServiceBinding(CreateScope, context.Parameter.ParameterType);

        private IServiceScope CreateScope(BindingContext bindingContext) => GetScope(bindingContext.FunctionInstanceId);

        public IServiceScope GetScope(BindingContext context)
        {
            return GetScope(context.FunctionInstanceId);
        }

        private IServiceScope GetScope(Guid id)
        {
            if (!_scopes.TryGetValue(id, out var scope))
            {
                scope = _serviceProvider.CreateScope();
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

        async Task IFunctionInvocationFilter.OnExecutingAsync(FunctionExecutingContext context, CancellationToken cancellationToken)
        {
            cancellationToken.Register(() => DisposeScope(context.FunctionInstanceId));
            //var scope = GetScope(context.FunctionInstanceId);

            //var filters = scope.ServiceProvider.GetRequiredService<IEnumerable<IFunctionFilter>>().OfType<IFunctionInvocationFilter>();
            //foreach (var filter in filters)
            //    await filter.OnExecutingAsync(context, cancellationToken);
        }

        async Task IFunctionInvocationFilter.OnExecutedAsync(FunctionExecutedContext context, CancellationToken cancellationToken)
        {
            DisposeScope(context.FunctionInstanceId);

            //var scope = GetScope(context.FunctionInstanceId);
            //var filters = scope.ServiceProvider.GetRequiredService<IEnumerable<IFunctionFilter>>().OfType<IFunctionInvocationFilter>();
            //foreach (var filter in filters)
            //    await filter.OnExecutedAsync(context, cancellationToken);
        }

        async Task IFunctionExceptionFilter.OnExceptionAsync(FunctionExceptionContext context, CancellationToken cancellationToken)
        {
            DisposeScope(context.FunctionInstanceId);

            //var scope = GetScope(context.FunctionInstanceId);
            //var filters = scope.ServiceProvider.GetRequiredService<IEnumerable<IFunctionFilter>>().OfType<IFunctionExceptionFilter>();
            //foreach (var filter in filters)
            //    await filter.OnExceptionAsync(context, cancellationToken);

        }
    }
}
