using System;
using Microsoft.Azure.WebJobs.Description;

namespace Rocket.Surgery.Azure.Functions
{
    [Binding, AttributeUsage(AttributeTargets.Parameter)]
    public class InjectAttribute : Attribute { }
    [Binding, AttributeUsage(AttributeTargets.Parameter)]
    public class ServiceAttribute : Attribute { }
    [Binding, AttributeUsage(AttributeTargets.Parameter)]
    public class _Attribute : Attribute { }
}
