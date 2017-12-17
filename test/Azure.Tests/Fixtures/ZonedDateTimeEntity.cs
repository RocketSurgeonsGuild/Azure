using NodaTime;

namespace Rocket.Surgery.Azure.Tests.Fixtures
{
    public class ZonedDateTimeEntity : Entity
    {
        public ZonedDateTime ZonedDateTime { get; set; }
    }
}
