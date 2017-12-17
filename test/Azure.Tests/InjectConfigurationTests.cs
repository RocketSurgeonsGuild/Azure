using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions;
using Microsoft.Azure.WebJobs.Host.Config;
using Newtonsoft.Json;
using Rocket.Surgery.Azure.Functions;
using Rocket.Surgery.Extensions.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Rocket.Surgery.Azure.Tests
{
    public class InjectConfigurationTests : AutoTestBase
    {
        public InjectConfigurationTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

        [Fact]
        public void Test1()
        {
            var config = new ServiceConfiguration();

            config.Initialize(new ExtensionConfigContext()
            {
                Config = new JobHostConfiguration()
                {
                    LoggerFactory = LoggerFactory
                },
                Trace = new TraceMonitor()
            });


        }
    }
}
