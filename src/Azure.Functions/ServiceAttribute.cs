using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace Rocket.Surgery.Azure.Functions
{
    [Binding, AttributeUsage(AttributeTargets.Parameter)]
    public class ServiceAttribute : Attribute { }
}
