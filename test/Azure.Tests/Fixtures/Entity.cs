using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rocket.Surgery.Azure.Storage;

namespace Rocket.Surgery.Azure.Tests.Fixtures
{
    public class Entity : ComplexTableEntity
    {
        public Entity() : base(nameof(Key), nameof(Key)) { }

        public string Key { get; set; }

        public int IntValue { get; set; }
        public long? LongValue { get; set; }
        public string StringValue { get; set; }
        public DateTimeOffset DateTimeOffsetValue { get; set; }

        protected override string GetPartitionKey() => Key;

        protected override void SetPartitionKey(string value) => Key = value;

        protected override string GetRowKey() => Key;

        protected override void SetRowKey(string value) => Key = value;

    }

    public class ExtensionEntity : ComplexTableEntity
    {
        public ExtensionEntity() : base(nameof(Key), nameof(Key)) { }
        public string Key { get; set; }
        protected override string GetPartitionKey() => Key;

        protected override void SetPartitionKey(string value) => Key = value;

        protected override string GetRowKey() => Key;

        protected override void SetRowKey(string value) => Key = value;

        [JsonExtensionData]
        public IDictionary<string, JToken> ExtensionData { get; set; }
    }
}
