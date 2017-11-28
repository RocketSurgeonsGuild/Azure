using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using FakeItEasy;
using Microsoft.Azure.WebJobs.Extensions;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Executors;
using Rocket.Surgery.Azure.Functions;
using Rocket.Surgery.Extensions.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Rocket.Surgery.Azure.Tests
{
    public class InjectBindingProviderTests : AutoTestBase
    {
        public InjectBindingProviderTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

        [Fact]
        public void Should_GetAScope()
        {
            var container = A.Fake<IContainer>();
            AutoFake.Provide(container);
            var provider = AutoFake.Resolve<InjectBindingProvider>();
            provider.GetScope(
                new BindingContext(
                    new ValueBindingContext(
                        new FunctionBindingContext(Guid.NewGuid(), CancellationToken.None, new TraceMonitor()),
                        CancellationToken.None
                    ),
                    new Dictionary<string, object>()
                )
            );

            A.CallTo(() => container.BeginLifetimeScope(A<object>._)).MustHaveHappened();
        }

        public async Task Should_CreateScope_OnExecuting()
        {
            var container = A.Fake<IContainer>();
            AutoFake.Provide(container);
            IFunctionInvocationFilter provider = AutoFake.Resolve<InjectBindingProvider>();

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

            A.CallTo(() => container.BeginLifetimeScope(A<object>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_CreateAScopeOnlyOnce()
        {
            var container = A.Fake<IContainer>();
            AutoFake.Provide(container);
            var provider = AutoFake.Resolve<InjectBindingProvider>();
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

            A.CallTo(() => container.BeginLifetimeScope(A<object>._)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async Task Should_DisposeOfAScope()
        {
            var container = A.Fake<IContainer>();
            AutoFake.Provide(container);
            var provider = AutoFake.Resolve<InjectBindingProvider>();
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

            A.CallTo(() => container.BeginLifetimeScope(A<object>._)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => scope.Dispose()).MustHaveHappened(Repeated.Exactly.Once);
        }
    }
}