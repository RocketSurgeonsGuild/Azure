using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using FakeItEasy;
using Microsoft.Azure.WebJobs.Extensions;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Rocket.Surgery.Extensions.Testing;
using Xunit;
using Xunit.Abstractions;
using Rocket.Surgery.Azure.Functions;

namespace Rocket.Surgery.Azure.Tests
{
    public class InjectBindingTests : AutoTestBase
    {
        public InjectBindingTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

        [Fact]
        public async Task ShouldBindAsExpected()
        {
            var context = A.Fake<Func<BindingContext, IComponentContext>>();
            A.CallTo(() => context(A<BindingContext>._)).Returns(AutoFake.Container);

            var binding = new InjectBinding(context, typeof(IComponentContext));

            await binding.BindAsync(
                new BindingContext(
                    new ValueBindingContext(
                        new FunctionBindingContext(Guid.NewGuid(), CancellationToken.None, new TraceMonitor()),
                        CancellationToken.None
                    ),
                    new Dictionary<string, object>()
                )
            );

            A.CallTo(() => context(A<BindingContext>._)).MustHaveHappened();
        }
    }
}
