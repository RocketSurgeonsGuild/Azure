using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
//using Microsoft.Azure.WebJobs.Hosting;
using Rocket.Surgery.Extensions.DependencyInjection;
using Rocket.Surgery.Reflection.Extensions;
using ExecutionContext = Microsoft.Azure.WebJobs.ExecutionContext;

namespace Rocket.Surgery.Azure.Functions
{
    //public static class FunctionExecutor
    //{
    //    private static readonly object SyncLock = new object();
    //    private static IServiceProvider _container;
    //    static IServiceProvider Init(ILogger logger, ExecutionContext executionContext)
    //    {
    //        lock (SyncLock)
    //        {
    //            if (_container != null) return _container;

    //            var environmentNames = new[]
    //            {
    //                Environment.GetEnvironmentVariable("WEBSITE_SLOT_NAME"),
    //                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
    //                "Unknown"
    //            };

    //            var applicationNames = new[]
    //            {
    //                Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME"),
    //                "Functions"
    //            };

    //            var envionment = new HostingEnvironment {
    //                EnvironmentName = environmentNames.First(x => !string.IsNullOrEmpty(x)),
    //                ApplicationName = applicationNames.First(x => !string.IsNullOrEmpty(x)),
    //                ContentRootPath = executionContext.FunctionAppDirectory,
    //                ContentRootFileProvider = null
    //            };

    //            var services = new ServiceCollection();

    //            var assemblyCandidateFinder = new AppDomainAssemblyCandidateFinder(AppDomain.CurrentDomain, logger);
    //            var assemblyProvider = new AppDomainAssemblyProvider(AppDomain.CurrentDomain, logger);
    //            var scanner = new AggregateConventionScanner(assemblyCandidateFinder);

    //            var extBuilder = new Microsoft.Extensions.Configuration.ConfigurationBuilder();
    //            var properties = new Dictionary<object, object>();
    //            var diagnosticSource = new DiagnosticListener("Rocket.Surgery.Azure");
    //            var configurationBuilder = new ConfigurationBuilder(
    //                scanner,
    //                envionment,
    //                new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build(),
    //                extBuilder,
    //                diagnosticSource,
    //                properties
    //            );

    //            configurationBuilder.Build();
    //            var configuration = EnvironmentVariablesExtensions.AddEnvironmentVariables(extBuilder)
    //                .Build();

    //            services.AddLogging();

    //            var builder = new AutofacBuilder(
    //                new ContainerBuilder(),
    //                scanner,
    //                assemblyProvider,
    //                assemblyCandidateFinder,
    //                services,
    //                configuration,
    //                envionment,
    //                diagnosticSource,
    //                properties
    //            );

    //            return _container = builder.Build().Resolve<IServiceProvider>();
    //        }
    //    }

    //    private static DisposableServiceProvider CreateContainer(Type type, ILogger logger,
    //        ExecutionContext executionContext, object[] additionalParameters)
    //    {
    //        var container = _container ?? Init(logger, executionContext);
    //        return new DisposableServiceProvider(
    //            container.GetRequiredService<IServiceScopeFactory>().CreateScope(),
    //            logger,
    //            executionContext,
    //            additionalParameters);
    //    }

    //    private static readonly ConcurrentDictionary<(Type type, string methodName), object> Methods =
    //        new ConcurrentDictionary<(Type type, string methodName), object>();

    //    private static Func<IServiceProvider, T> GetMethod<T>(Type type, string methodName)
    //    {
    //        return (Func<IServiceProvider, T>)Methods.GetOrAdd((type, methodName), x => InjectableMethodBuilder.Create(x.type, x.methodName).CompileStatic<T>());
    //    }

    //    public static T Run<T>(Type type, string methodName, ILogger logger, ExecutionContext executionContext, params object[] additionalParameters)
    //    {
    //        using (var container = CreateContainer(type, logger, executionContext, additionalParameters))
    //            return GetMethod<T>(type, methodName)(container);
    //    }

    //    private static Action<IServiceProvider> GetMethod(Type type, string methodName)
    //    {
    //        return (Action<IServiceProvider>)Methods.GetOrAdd((type, methodName), x => InjectableMethodBuilder.Create(x.type, x.methodName).CompileStatic());
    //    }

    //    public static void Run(Type type, string methodName, ILogger logger, ExecutionContext executionContext, params object[] additionalParameters)
    //    {
    //        using (var container = CreateContainer(type, logger, executionContext, additionalParameters))
    //            GetMethod(type, methodName)(container);
    //    }

    //    private static Func<IServiceProvider, Task<T>> GetMethodAsync<T>(Type type, string methodName)
    //    {
    //        return (Func<IServiceProvider, Task<T>>)Methods.GetOrAdd((type, methodName), x => InjectableMethodBuilder.Create(x.type, x.methodName).CompileStatic<Task<T>>());
    //    }

    //    public static Task<T> RunAsync<T>(Type type, string methodName, ILogger logger, ExecutionContext executionContext, params object[] additionalParameters)
    //    {
    //        using (var container = CreateContainer(type, logger, executionContext, additionalParameters))
    //            return GetMethodAsync<T>(type, methodName)(container);
    //    }

    //    private static Func<IServiceProvider, Task> GetMethodAsync(Type type, string methodName)
    //    {
    //        return (Func<IServiceProvider, Task>)Methods.GetOrAdd((type, methodName), x => InjectableMethodBuilder.Create(x.type, x.methodName).CompileStatic<Task>());
    //    }

    //    public static Task RunAsync(Type type, string methodName, ILogger logger, ExecutionContext executionContext, params object[] additionalParameters)
    //    {
    //        using (var container = CreateContainer(type, logger, executionContext, additionalParameters))
    //            return GetMethodAsync(type, methodName)(container);
    //    }
    //}

    //public class RocketSurgeryWebJobsStartup : IWebJobsStartup
    //{
    //    public void Configure(IWebJobsBuilder builder)
    //    {
    //        builder.AddRocketSurgery();
    //    }
    //}
}
