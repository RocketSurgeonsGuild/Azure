using NodaTime;

namespace Rocket.Surgery.Azure.Tests.Fixtures
{
    public class InstantEntity : Entity
    {
        public Instant Instant { get; set; }
    }
}
