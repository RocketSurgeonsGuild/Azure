using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NodaTime;
using NodaTime.Serialization.JsonNet;
using NodaTime.Text;
using NodaTime.Utility;

namespace Rocket.Surgery.Azure.Storage.Converters
{
    /// <summary>
    /// A JSON converter for types which can be represented by a single string value, parsed or formatted
    /// from an <see cref="IPattern{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type to convert to/from JSON.</typeparam>
    public sealed class InstantPatternConverter : NodaConverterBase<Instant>
    {
        public IEnumerable<IPattern<Instant>> Patterns { get; }
        public IEnumerable<IPattern<OffsetDateTime>> AlternatePatterns { get; }

        /// <summary>
        /// Creates a new instance with a pattern and an optional validator. The validator will be called before each
        /// value is written, and may throw an exception to indicate that the value cannot be serialized.
        /// </summary>
        /// <param name="pattern">The pattern to use for parsing and formatting.</param>
        /// <param name="validator">The validator to call before writing values. May be null, indicating that no validation is required.</param>
        /// <exception cref="ArgumentNullException"><paramref name="pattern"/> is null.</exception>
        public InstantPatternConverter(IPattern<Instant>[] patterns, IPattern<OffsetDateTime>[] alternatePatterns)
        {
            // Note: We could use Preconditions.CheckNotNull, but only if we either made that public in NodaTime
            // or made InternalsVisibleTo this assembly.
            if (!patterns.Any())
            {
                throw new ArgumentNullException(nameof(patterns));
            }
            this.Patterns = patterns;
            AlternatePatterns = alternatePatterns;
        }

        /// <summary>
        /// Implemented by concrete subclasses, this performs the final conversion from a non-null JSON value to
        /// a value of type T.
        /// </summary>
        /// <param name="reader">The JSON reader to pull data from</param>
        /// <param name="serializer">The serializer to use for nested serialization</param>
        /// <returns>The deserialized value of type T.</returns>
        protected override Instant ReadJsonImpl(JsonReader reader, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.String)
            {
                throw new InvalidNodaDataException(
                    $"Unexpected token parsing {typeof(Instant).Name}. Expected String, got {reader.TokenType}.");
            }
            string text = reader.Value.ToString();

            var mainResult = Patterns
                .Select(x => x.Parse(text))
                .FirstOrDefault(z => z.Success);

            if (mainResult?.Success == true) return mainResult.Value;

            var secondaryResult = AlternatePatterns
                .Select(z => z.Parse(text))
                .FirstOrDefault(z => z.Success);

            if (secondaryResult?.Success == true)
            {
                return secondaryResult.Value.ToInstant();
            }
            return Patterns.First().Parse(text).Value;
        }

        /// <summary>
        /// Writes the formatted value to the writer.
        /// </summary>
        /// <param name="writer">The writer to write JSON data to</param>
        /// <param name="value">The value to serializer</param>
        /// <param name="serializer">The serializer to use for nested serialization</param>
        protected override void WriteJsonImpl(JsonWriter writer, Instant value, JsonSerializer serializer)
        {
            writer.WriteValue(Patterns.First().Format(value));
        }
    }
}
