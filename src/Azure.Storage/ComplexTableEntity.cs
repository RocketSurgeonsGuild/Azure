using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using NodaTime;
using NodaTime.Serialization.JsonNet;
using NodaTime.Text;
using Rocket.Surgery.Azure.Storage.Converters;
using Rocket.Surgery.Binding;
using Rocket.Surgery.Reflection.Extensions;

namespace Rocket.Surgery.Azure.Storage
{
    /// <summary>
    /// Class ComplexTableEntity.
    /// </summary>
    /// <seealso cref="Microsoft.WindowsAzure.Storage.Table.ITableEntity" />
    /// TODO Edit XML Comment Template for ComplexTableEntity
    public abstract class ComplexTableEntity : ITableEntity
    {
        private readonly string _partitionKey;
        private readonly string _rowKey;
        private readonly JsonSerializer _jsonSerializer;
        private const string _separator = "__";
        private readonly static ConcurrentDictionary<Type, PropertyGetter> PropertyGetters = new ConcurrentDictionary<Type, PropertyGetter>();

        /// <summary>
        /// The json serializer
        /// </summary>
        /// TODO Edit XML Comment Template for JsonSerializer
        public static JsonSerializer JsonSerializer = JsonSerializer
            .CreateDefault(new JsonSerializerSettings()
            {
                ContractResolver = new PrivateSetterContractResolver(),
                Converters = new List<JsonConverter>()
                {
                    new StringEnumConverter()
                }
            })
            .ConfigureForNodaTime(DateTimeZoneProviders.Tzdb)
            .ConfigureForComplexTableEntity(DateTimeZoneProviders.Tzdb);

        private readonly JsonBinder _binder;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComplexTableEntity"/> class.
        /// </summary>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="rowKey">The row key.</param>
        /// <param name="jsonSerializer">The json serializer.</param>
        /// <exception cref="ArgumentNullException">
        /// partitionKey
        /// or
        /// rowKey
        /// or
        /// jsonSerializer
        /// </exception>
        /// TODO Edit XML Comment Template for #ctor
        protected ComplexTableEntity(string partitionKey, string rowKey, JsonSerializer jsonSerializer)
        {
            _partitionKey = partitionKey ?? throw new ArgumentNullException(nameof(partitionKey));
            _rowKey = rowKey ?? throw new ArgumentNullException(nameof(rowKey));
            _jsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
            _binder = new JsonBinder(_separator, _jsonSerializer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComplexTableEntity"/> class.
        /// </summary>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="rowKey">The row key.</param>
        /// TODO Edit XML Comment Template for #ctor
        protected ComplexTableEntity(string partitionKey, string rowKey) : this(partitionKey, rowKey, JsonSerializer)
        {
        }

        void ITableEntity.ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            var getter = getPropertyGetter(GetType());
            _binder.Populate(this, properties
                .Select(x =>
                {
                    var value = x.Value.PropertyAsObject?.ToString();
                    if (x.Value.PropertyType != EdmType.DateTime) return new KeyValuePair<string, string>(x.Key, value);

                    if (x.Value.DateTimeOffsetValue.HasValue)
                    {
                        if (TryGetPropertyType(getter, GetType(), x.Key, out var propertyType))
                        {
                            if (propertyType == typeof(Instant))
                            {
                                value = InstantPattern.ExtendedIso.Format(
                                    Instant.FromDateTimeOffset(x.Value.DateTimeOffsetValue.Value));
                            }
                            else if (propertyType == typeof(OffsetDateTime))
                            {
                                value = OffsetDateTimePattern.Rfc3339.Format(
                                    OffsetDateTime.FromDateTimeOffset(x.Value.DateTimeOffsetValue.Value));
                            }
                            else if (propertyType == typeof(ZonedDateTime))
                            {
                                value = CustomNodaConverters.ZonedDateTimeFormatter.Format(
                                    ZonedDateTime.FromDateTimeOffset(x.Value.DateTimeOffsetValue.Value));
                            }
                            else if (propertyType == typeof(LocalDateTime))
                            {
                                value = LocalDateTimePattern.ExtendedIso.Format(
                                    LocalDateTime.FromDateTime(x.Value.DateTimeOffsetValue.Value.DateTime));
                            }
                            else if (propertyType == typeof(LocalDate))
                            {
                                value = LocalDatePattern.Iso.Format(
                                    LocalDate.FromDateTime(x.Value.DateTimeOffsetValue.Value.DateTime));
                            }
                            else
                            {
                                value = x.Value.DateTimeOffsetValue.Value.ToString("O");
                            }
                        }
                        else
                        {
                            value = x.Value.DateTimeOffsetValue.Value.ToString("O");
                        }
                    }

                    if (x.Value.DateTime.HasValue)
                    {
                        if (TryGetPropertyType(getter, GetType(), x.Key, out var propertyType))
                        {
                            if (propertyType == typeof(LocalDateTime))
                            {
                                value = LocalDateTimePattern.ExtendedIso.Format(
                                    LocalDateTime.FromDateTime(x.Value.DateTime.Value));
                            }
                            else if (propertyType == typeof(LocalDate))
                            {
                                value = LocalDatePattern.Iso.Format(LocalDate.FromDateTime(x.Value.DateTime.Value));
                            }
                            else
                            {
                                value = x.Value.DateTime.Value.ToString("O");
                            }
                        }
                        else
                        {
                            value = x.Value.DateTime.Value.ToString("O");
                        }
                    }
                    return new KeyValuePair<string, string>(x.Key, value);
                }));
        }

        private static bool TryGetPropertyType(PropertyGetter getter, Type baseType, string key, out Type type)
        {
            try
            {
                type = getter.GetPropertyType(baseType, key);
                return true;
            }
            catch
            {
                type = null;
                return false;
            }
        }

        private static PropertyGetter getPropertyGetter(Type type)
        {
            if (!PropertyGetters.TryGetValue(type, out var getter))
            {
                getter = new PropertyGetter(_separator);
                PropertyGetters.TryAdd(type, getter);
            }
            return getter;
        }

        IDictionary<string, EntityProperty> ITableEntity.WriteEntity(OperationContext operationContext)
        {
            var getter = getPropertyGetter(GetType());
            var results = new Dictionary<string, EntityProperty>();
            foreach (var (key, item) in _binder.GetValues(this))
            {
                if (key == _rowKey || key == _partitionKey) continue;
                if (key == nameof(PartitionKey) || key == nameof(RowKey)) continue;
                EntityProperty entityProperty = null;

                switch (item.Type)
                {
                    case JTokenType.Null:
                        entityProperty = new EntityProperty((string)null);
                        break;
                    case JTokenType.Boolean:
                        entityProperty = EntityProperty.GeneratePropertyForBool(item.Value<bool>());
                        break;
                    case JTokenType.Date:
                        entityProperty = EntityProperty.GeneratePropertyForDateTimeOffset(item.Value<DateTimeOffset>());
                        break;
                    case JTokenType.Guid:
                        entityProperty = EntityProperty.GeneratePropertyForGuid(item.Value<Guid>());
                        break;
                    case JTokenType.Integer:
                        switch (item.Value)
                        {
                            case short s:
                                entityProperty = EntityProperty.GeneratePropertyForInt(s);
                                break;
                            case int i:
                                entityProperty = EntityProperty.GeneratePropertyForInt(i);
                                break;
                            case long l:
                                entityProperty = EntityProperty.GeneratePropertyForLong(l);
                                break;
                        }
                        break;
                    case JTokenType.Float:
                        switch (item.Value)
                        {
                            case float f:
                                entityProperty = EntityProperty.GeneratePropertyForDouble(f);
                                break;
                            case double d:
                                entityProperty = EntityProperty.GeneratePropertyForDouble(d);
                                break;
                            case decimal _:
                                entityProperty = EntityProperty.GeneratePropertyForString(item.Value<string>());
                                break;
                        }
                        break;

                    default:
                        if (TryGetPropertyType(getter, GetType(), key, out var _))
                        {
                            var context = getter.Get(this, key);

                            switch (context)
                            {
                                case null:
                                    entityProperty = new EntityProperty((string)null);
                                    break;
                                case Instant instant:
                                    entityProperty = EntityProperty.GeneratePropertyForDateTimeOffset(instant.ToDateTimeOffset());
                                    break;
                                case LocalDate localDate:
                                    entityProperty = EntityProperty.GeneratePropertyForDateTimeOffset(localDate.ToDateTimeUnspecified());
                                    break;
                                case LocalDateTime localDateTime:
                                    entityProperty = EntityProperty.GeneratePropertyForDateTimeOffset(localDateTime.ToDateTimeUnspecified());
                                    break;
                                case OffsetDateTime offsetDateTime:
                                    entityProperty = EntityProperty.GeneratePropertyForDateTimeOffset(offsetDateTime.ToDateTimeOffset());
                                    break;
                                case ZonedDateTime zonedDateTime:
                                    entityProperty = EntityProperty.GeneratePropertyForDateTimeOffset(zonedDateTime.ToDateTimeOffset());
                                    break;
                                default:
                                    entityProperty = EntityProperty.GeneratePropertyForString(item.Value<string>());
                                    break;
                            }
                        }
                        else
                        {
                            entityProperty = EntityProperty.GeneratePropertyForString(item.Value<string>());
                        }
                        break;
                }

                results.Add(key, entityProperty);
            }
            return results;
        }

        public string PartitionKey => ((ITableEntity)this).PartitionKey;

        string ITableEntity.PartitionKey
        {
            get => GetPartitionKey();
            set => SetPartitionKey(value);
        }

        /// <summary>
        /// Gets the partition key.
        /// </summary>
        /// <returns>System.String.</returns>
        /// TODO Edit XML Comment Template for GetPartitionKey
        protected abstract string GetPartitionKey();

        /// <summary>
        /// Sets the partition key.
        /// </summary>
        /// <param name="value">The value.</param>
        /// TODO Edit XML Comment Template for SetPartitionKey
        protected abstract void SetPartitionKey(string value);

        public string RowKey => ((ITableEntity)this).RowKey;

        string ITableEntity.RowKey
        {
            get => GetRowKey();
            set => SetRowKey(value);
        }

        /// <summary>
        /// Gets the row key.
        /// </summary>
        /// <returns>System.String.</returns>
        /// TODO Edit XML Comment Template for GetRowKey
        protected abstract string GetRowKey();

        /// <summary>
        /// Sets the row key.
        /// </summary>
        /// <param name="value">The value.</param>
        /// TODO Edit XML Comment Template for SetRowKey
        protected abstract void SetRowKey(string value);

        DateTimeOffset ITableEntity.Timestamp { get; set; }
        string ITableEntity.ETag { get; set; }
    }
}
