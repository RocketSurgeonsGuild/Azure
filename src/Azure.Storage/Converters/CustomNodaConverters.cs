using System;
using NodaTime;
using NodaTime.Text;

namespace Rocket.Surgery.Azure.Storage.Converters
{
    public static class CustomNodaConverters
    {
        public static InstantPatternConverter InstantConverter { get; }
            = new InstantPatternConverter(
                new[] {
                    InstantPattern.ExtendedIso,
                    InstantPattern.General
                },
                new[] {
                    OffsetDateTimePattern.Rfc3339,
                    OffsetDateTimePattern.ExtendedIso,
                    OffsetDateTimePattern.GeneralIso
                }
            );

        public static NodaMultiplePatternConverter<LocalDateTime> LocalDateTimeConverter { get; }
            = new NodaMultiplePatternConverter<LocalDateTime>(
                CreateIsoValidator<LocalDateTime>(x => x.Calendar),
                LocalDateTimePattern.ExtendedIso,
                LocalDateTimePattern.GeneralIso,
                LocalDateTimePattern.FullRoundtrip,
                LocalDateTimePattern.BclRoundtrip
            );


        public static OffsetDateTimePatternConverter OffsetDateTimeConverter { get; } =
            new OffsetDateTimePatternConverter(
                CreateIsoValidator<OffsetDateTime>(x => x.Calendar),
                new IPattern<OffsetDateTime>[] {
                    OffsetDateTimePattern.Rfc3339,
                    OffsetDateTimePattern.ExtendedIso,
                    OffsetDateTimePattern.GeneralIso,
                },
                new IPattern<Instant>[] {
                    InstantPattern.ExtendedIso,
                    InstantPattern.General
                }
            );

        public static IPattern<ZonedDateTime> ZonedDateTimeFormatter = ZonedDateTimePattern.CreateWithInvariantCulture("uuuu'-'MM'-'dd'T'HH':'mm':'ss;FFFFFFFFFo<G> z", DateTimeZoneProviders.Tzdb);


        private static Action<T> CreateIsoValidator<T>(Func<T, CalendarSystem> calendarProjection) => value =>
        {
            var calendar = calendarProjection(value);
            // We rely on CalendarSystem.Iso being a singleton here.
            if (calendar != CalendarSystem.Iso)
            {
                throw new ArgumentException(
                    $"Values of type {typeof(T).Name} must (currently) use the ISO calendar in order to be serialized.");
            }
        };
    }
}
