using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
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
    public sealed class NodaMultiplePatternConverter<T> : NodaConverterBase<T>
    {
        public IEnumerable<IPattern<T>> Patterns { get; }
        private readonly Action<T> validator;

        /// <summary>
        /// Creates a new instance with a pattern and no validator.
        /// </summary>
        /// <param name="pattern">The pattern to use for parsing and formatting.</param>
        /// <exception cref="ArgumentNullException"><paramref name="pattern"/> is null.</exception>
        public NodaMultiplePatternConverter(params IPattern<T>[] patterns) : this(null, patterns)
        {
        }

        /// <summary>
        /// Creates a new instance with a pattern and an optional validator. The validator will be called before each
        /// value is written, and may throw an exception to indicate that the value cannot be serialized.
        /// </summary>
        /// <param name="pattern">The pattern to use for parsing and formatting.</param>
        /// <param name="validator">The validator to call before writing values. May be null, indicating that no validation is required.</param>
        /// <exception cref="ArgumentNullException"><paramref name="pattern"/> is null.</exception>
        public NodaMultiplePatternConverter(Action<T> validator, params IPattern<T>[] patterns)
        {
            // Note: We could use Preconditions.CheckNotNull, but only if we either made that public in NodaTime
            // or made InternalsVisibleTo this assembly.
            if (!patterns.Any())
            {
                throw new ArgumentNullException(nameof(patterns));
            }
            this.Patterns = patterns;
            this.validator = validator;
        }

        /// <summary>
        /// Implemented by concrete subclasses, this performs the final conversion from a non-null JSON value to
        /// a value of type T.
        /// </summary>
        /// <param name="reader">The JSON reader to pull data from</param>
        /// <param name="serializer">The serializer to use for nested serialization</param>
        /// <returns>The deserialized value of type T.</returns>
        protected override T ReadJsonImpl(JsonReader reader, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.String)
            {
                throw new InvalidNodaDataException(
                    $"Unexpected token parsing {typeof(T).Name}. Expected String, got {reader.TokenType}.");
            }
            string text = reader.Value.ToString();
            return Patterns.Select(x => x.Parse(text)).First(z => z.Success).Value;
        }

        /// <summary>
        /// Writes the formatted value to the writer.
        /// </summary>
        /// <param name="writer">The writer to write JSON data to</param>
        /// <param name="value">The value to serializer</param>
        /// <param name="serializer">The serializer to use for nested serialization</param>
        protected override void WriteJsonImpl(JsonWriter writer, T value, JsonSerializer serializer)
        {
            validator?.Invoke(value);
            writer.WriteValue(Patterns.First().Format(value));
        }
    }
}
