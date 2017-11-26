using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using NodaTime;
using NodaTime.Serialization.JsonNet;
using NodaTime.Text;
using Rocket.Surgery.Binding;

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
            }).ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

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
            _binder = new JsonBinder("__");
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
            var jobject = _binder.Parse(properties.Select(
                x =>
                {
                    var value = x.Value.PropertyAsObject.ToString();
                    if (x.Value.PropertyType == EdmType.DateTime)
                    {
                        if (x.Value.DateTimeOffsetValue.HasValue)
                        {
                            value = x.Value.DateTimeOffsetValue.Value.ToString("O");
                        }
                        else if (x.Value.DateTime.HasValue)
                        {
                            value = x.Value.DateTime.Value.ToUniversalTime().ToString("O");
                        }
                    }
                    else
                    {
                        if (DateTimeOffset.TryParse(value, out var dateTimeOffset))
                        {
                            value = dateTimeOffset.ToString("O");
                        }
                        if (DateTime.TryParse(value, out var dateTime))
                        {
                            value = dateTime.ToString("O");
                        }
                    }
                    return new KeyValuePair<string, string>(x.Key, value);
                }));

            _jsonSerializer.Populate(jobject.CreateReader(), this);
        }

        IDictionary<string, EntityProperty> ITableEntity.WriteEntity(OperationContext operationContext)
        {
            var results = new Dictionary<string, EntityProperty>();
            foreach (var item in JObject.FromObject(this, _jsonSerializer)
                .Descendants()
                .Where(p => !p.Any())
                .OfType<JValue>())
            {
                var key = _binder.GetKey(item);
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
                        if (item.Value is long)
                            entityProperty = EntityProperty.GeneratePropertyForLong(item.Value<long>());
                        if (item.Value is int)
                            entityProperty = EntityProperty.GeneratePropertyForInt(item.Value<int>());
                        if (item.Value is short)
                            entityProperty = EntityProperty.GeneratePropertyForInt(item.Value<short>());
                        break;
                    case JTokenType.Float:
                        if (item.Value is decimal)
                            entityProperty = EntityProperty.GeneratePropertyForString(item.Value<string>());
                        if (item.Value is double)
                            entityProperty = EntityProperty.GeneratePropertyForDouble(item.Value<double>());
                        if (item.Value is float)
                            entityProperty = EntityProperty.GeneratePropertyForDouble(item.Value<float>());
                        break;
                    case JTokenType.Bytes:
                        entityProperty = EntityProperty.GeneratePropertyForByteArray(item.Value<byte[]>());
                        break;
                    default:
                        var value = item.Value<string>();

                        var offsetDateTime = OffsetDateTimePattern.Rfc3339.Parse(value);
                        if (offsetDateTime.Success)
                        {
                            entityProperty = EntityProperty.GeneratePropertyForDateTimeOffset(offsetDateTime.Value.ToDateTimeOffset());
                            break;
                        }
                        var instant = InstantPattern.ExtendedIso.Parse(value);
                        if (instant.Success)
                        {
                            entityProperty = EntityProperty.GeneratePropertyForDateTimeOffset(instant.Value.ToDateTimeOffset());
                            break;
                        }
                        entityProperty = EntityProperty.GeneratePropertyForString(item.Value<string>());
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
