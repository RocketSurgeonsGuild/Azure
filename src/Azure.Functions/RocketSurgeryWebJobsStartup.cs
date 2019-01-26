using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Rocket.Surgery.Azure.Functions;
using Rocket.Surgery.Conventions;
//using Microsoft.Azure.WebJobs.Hosting;
using Rocket.Surgery.Extensions.DependencyInjection;
using Rocket.Surgery.Reflection.Extensions;
using ExecutionContext = Microsoft.Azure.WebJobs.ExecutionContext;
using Rocket.Surgery.Hosting;

//[assembly: WebJobsStartup(typeof(RocketSurgeryWebJobsStartup))]

namespace Rocket.Surgery.Azure.Functions
{
    public abstract class RocketSurgeryWebJobsStartup : IWebJobsStartup
    {
        private readonly Assembly _assembly;

        public RocketSurgeryWebJobsStartup(Assembly assembly)
        {
            _assembly = assembly;
        }

        public virtual void Configure(IWebJobsBuilder builder)
        {
            builder.AddRocketSurgery(_assembly, this, OnBuild);
        }

        protected abstract void OnBuild(IConventionHostBuilder builder);
    }
}
