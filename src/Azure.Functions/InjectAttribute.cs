using System;
using Microsoft.Azure.WebJobs.Description;

namespace Rocket.Surgery.Azure.Functions
{

    [Binding]
    [AttributeUsage(AttributeTargets.Parameter)]
    public class InjectAttribute : Attribute
    {
    }
}
