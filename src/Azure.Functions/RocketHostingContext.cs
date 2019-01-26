using Rocket.Surgery.Conventions;
//using Microsoft.Azure.WebJobs.Hosting;
using Rocket.Surgery.Hosting;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;
using System.Collections.Generic;
using System.Diagnostics;
using System;

//[assembly: WebJobsStartup(typeof(RocketSurgeryWebJobsStartup))]

namespace Rocket.Surgery.Azure.Functions
{
    class RocketHostingContext : ConventionHostBuilder, IRocketHostingContext, IConventionHostBuilder
    {
        public RocketHostingContext(
            IConventionScanner scanner,
            IAssemblyCandidateFinder assemblyCandidateFinder,
            IAssemblyProvider assemblyProvider,
            DiagnosticSource diagnosticSource,
            IDictionary<object, object> properties): base(scanner, assemblyCandidateFinder, assemblyProvider, diagnosticSource, properties)
        {
            Scanner = scanner;
            AssemblyProvider = assemblyProvider;
            AssemblyCandidateFinder = assemblyCandidateFinder;
            DiagnosticSource = diagnosticSource;
            Properties = properties;
        }

        public IConventionScanner Scanner { get; }

        public DiagnosticSource DiagnosticSource { get; }

        public IDictionary<object, object> Properties { get; }

        public IAssemblyProvider AssemblyProvider { get; }

        public IAssemblyCandidateFinder AssemblyCandidateFinder { get; }

        public string[] Arguments { get; } = Array.Empty<string>();
    }
}
