using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using FakeItEasy;
using Microsoft.Azure.WebJobs.Extensions;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.Azure.Functions;
using Rocket.Surgery.Extensions.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Rocket.Surgery.Azure.Tests
{
    public class InjectBindingProviderTests : AutoTestBase
    {
        public InjectBindingProviderTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            var container = AutoFake.Resolve<IServiceProvider>();
            A.CallTo(() => container.GetService(A<Type>._))
                .ReturnsLazily(call => AutoFake.Container.Resolve(call.Arguments.First() as Type));
        }

        [Fact]
        public void Should_GetAScope()
        {
            var container = AutoFake.Resolve<IServiceProvider>();
            var provider = AutoFake.Resolve<ServiceBindingProvider>();
            provider.GetScope(
                new BindingContext(
                    new ValueBindingContext(
                        new FunctionBindingContext(Guid.NewGuid(), CancellationToken.None, new TraceMonitor()),
                        CancellationToken.None
                    ),
                    new Dictionary<string, object>()
                )
            );

            A.CallTo(() => container.GetService(typeof(IServiceScopeFactory))).MustHaveHappened();
        }

        public async Task Should_CreateScope_OnExecuting()
        {
            var container = AutoFake.Resolve<IServiceProvider>();
            IFunctionInvocationFilter provider = AutoFake.Resolve<ServiceBindingProvider>();

            await provider.OnExecutedAsync(
                new FunctionExecutedContext(
                    new Dictionary<string, object>(),
                    new Dictionary<string, object>(),
                    Guid.NewGuid(),
                    "name",
                    Logger,
                    new FunctionResult(true)
                ),
                CancellationToken.None
            );

            A.CallTo(() => container.GetService(typeof(IServiceScopeFactory))).MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_CreateAScopeOnlyOnce()
        {
            var container = AutoFake.Resolve<IServiceProvider>();
            var provider = AutoFake.Resolve<ServiceBindingProvider>();
            IFunctionInvocationFilter filter = provider;

            var id = Guid.NewGuid();

            await filter.OnExecutingAsync(
                new FunctionExecutingContext(

                    new Dictionary<string, object>(),
                    new Dictionary<string, object>(),
                    id,
                    "name",
                    Logger
                ),
                CancellationToken.None
            );

            provider.GetScope(
                new BindingContext(
                    new ValueBindingContext(
                        new FunctionBindingContext(id, CancellationToken.None, new TraceMonitor()),
                        CancellationToken.None
                    ),
                    new Dictionary<string, object>()
                )
            );

            A.CallTo(() => container.GetService(typeof(IServiceScopeFactory))).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async Task Should_DisposeOfAScope()
        {
            var container = AutoFake.Resolve<IServiceProvider>();
            var provider = AutoFake.Resolve<ServiceBindingProvider>();
            IFunctionInvocationFilter filter = provider;

            var id = Guid.NewGuid();

            await filter.OnExecutingAsync(
                new FunctionExecutingContext(
                    new Dictionary<string, object>(),
                    new Dictionary<string, object>(),
                    id,
                    "name",
                    Logger
                ),
                CancellationToken.None
            );

            var scope = provider.GetScope(
                new BindingContext(
                    new ValueBindingContext(
                        new FunctionBindingContext(id, CancellationToken.None, new TraceMonitor()),
                        CancellationToken.None
                    ),
                    new Dictionary<string, object>()
                )
            );

            await filter.OnExecutedAsync(
                new FunctionExecutedContext(
                    new Dictionary<string, object>(),
                    new Dictionary<string, object>(),
                    id,
                    "name",
                    Logger,
                    new FunctionResult(true)
                ),
                CancellationToken.None
            );

            A.CallTo(() => container.GetService(typeof(IServiceScopeFactory))).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => scope.Dispose()).MustHaveHappened(Repeated.Exactly.Once);
        }
    }
}
