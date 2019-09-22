using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json.Linq;
using NodaTime;
using NodaTime.Text;
using Rocket.Surgery.Azure.Storage.Converters;
using Rocket.Surgery.Azure.Tests.Fixtures;
using Rocket.Surgery.Extensions.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Rocket.Surgery.Azure.Tests
{
    public class ComplexTableEntityTests : AutoFakeTest
    {
        public ComplexTableEntityTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

        [Fact]
        public void Should_ReadEntity()
        {
            var entity = new Entity();
            var tEntity = (ITableEntity)entity;

            tEntity.ReadEntity(new Dictionary<string, EntityProperty>
            {
                { nameof(Entity.IntValue), EntityProperty.GeneratePropertyForInt(123) },
                { nameof(Entity.LongValue), EntityProperty.GeneratePropertyForLong(null) },
                { nameof(Entity.StringValue), EntityProperty.GeneratePropertyForString("Abc") },
                { nameof(Entity.DateTimeOffsetValue), EntityProperty.GeneratePropertyForDateTimeOffset(DateTimeOffset.Now) },
            }, new OperationContext());

            entity.IntValue.Should().Be(123);
            entity.LongValue.Should().BeNull();
            entity.StringValue.Should().Be("Abc");
            entity.DateTimeOffsetValue.Should().BeOnOrBefore(DateTimeOffset.Now);
        }

        [Fact]
        public void Should_WriteEntity()
        {
            var entity = new Entity()
            {
                Key = "Abc",
                IntValue = 123,
                LongValue = 456L,
                DateTimeOffsetValue = DateTimeOffset.Now,
                StringValue = "Abc"
            };
            var tEntity = (ITableEntity)entity;

            var result = tEntity.WriteEntity(new OperationContext());

            result[nameof(Entity.IntValue)].PropertyAsObject.Should().Be(123);
            result[nameof(Entity.LongValue)].PropertyAsObject.Should().Be(456L);
            result[nameof(Entity.DateTimeOffsetValue)].PropertyAsObject.Should().NotBeNull();
            result[nameof(Entity.StringValue)].PropertyAsObject.Should().Be("Abc");
            result.Should().NotContainKey(nameof(entity.Key));
            result.Should().NotContainKey(nameof(entity.PartitionKey));
            result.Should().NotContainKey(nameof(entity.RowKey));
        }

        [Fact]
        public void Should_ReadExtensionEntity()
        {
            var entity = new ExtensionEntity();
            var tEntity = (ITableEntity)entity;

            tEntity.ReadEntity(new Dictionary<string, EntityProperty>
            {
                { nameof(Entity.IntValue), EntityProperty.GeneratePropertyForInt(123) },
                { nameof(Entity.LongValue), EntityProperty.GeneratePropertyForLong(null) },
                { nameof(Entity.StringValue), EntityProperty.GeneratePropertyForString("Abc") },
                { nameof(Entity.DateTimeOffsetValue), EntityProperty.GeneratePropertyForDateTimeOffset(DateTimeOffset.Now) },
            }, new OperationContext());

            entity.ExtensionData[nameof(Entity.IntValue)]
                .ToObject<int>().Should().Be(123);
            entity.ExtensionData[nameof(Entity.LongValue)]
                .ToObject<long?>().Should().BeNull();
            entity.ExtensionData[nameof(Entity.StringValue)]
                .ToObject<string>().Should().Be("Abc");
            entity.ExtensionData[nameof(Entity.DateTimeOffsetValue)]
                .ToObject<DateTimeOffset>().Should().BeOnOrBefore(DateTimeOffset.Now);
        }

        [Fact]
        public void Should_WriteExtensionEntity()
        {
            var entity = new ExtensionEntity()
            {
                Key = "Abc",
                ExtensionData = new Dictionary<string, JToken>()
                {
                    { nameof(Entity.IntValue), 123 },
                    { nameof(Entity.LongValue), null },
                    { nameof(Entity.StringValue), "Abc" },
                    { nameof(Entity.DateTimeOffsetValue), DateTimeOffset.Now },
                }
            };
            var tEntity = (ITableEntity)entity;

            var result = tEntity.WriteEntity(new OperationContext());

            result[nameof(Entity.IntValue)].PropertyAsObject.Should().Be(123L);
            result[nameof(Entity.LongValue)].PropertyAsObject.Should().BeNull();
            result[nameof(Entity.DateTimeOffsetValue)].PropertyAsObject.Should().NotBeNull();
            result[nameof(Entity.StringValue)].PropertyAsObject.Should().Be("Abc");
            result.Should().NotContainKey(nameof(entity.Key));
            result.Should().NotContainKey(nameof(entity.PartitionKey));
            result.Should().NotContainKey(nameof(entity.RowKey));
        }

        [Fact]
        public void Should_ReadInstantEntity()
        {
            var date = DateTimeOffset.Now;
            var entity = new InstantEntity();
            var tEntity = (ITableEntity)entity;

            tEntity.ReadEntity(new Dictionary<string, EntityProperty>
            {
                {
                    nameof(InstantEntity.Instant),
                    EntityProperty.GeneratePropertyForString(InstantPattern.ExtendedIso.Format(Instant.FromDateTimeOffset(date)))
                }
            }, new OperationContext());

            entity.Instant.Should().BeLessOrEqualTo(Instant.FromDateTimeOffset(date));
        }

        [Fact]
        public void Should_ReadInstantEntity_FromDateTimeOffset()
        {
            var date = DateTimeOffset.Now;
            var entity = new InstantEntity();
            var tEntity = (ITableEntity)entity;

            tEntity.ReadEntity(new Dictionary<string, EntityProperty>
            {
                {
                    nameof(InstantEntity.Instant),
                    EntityProperty.GeneratePropertyForDateTimeOffset(date)
                }
            }, new OperationContext());

            entity.Instant.Should().BeLessOrEqualTo(Instant.FromDateTimeOffset(date));
        }

        [Fact]
        public void Should_WriteInstantEntity()
        {
            var date = DateTimeOffset.Now.ToOffset(TimeSpan.Zero);
            var entity = new InstantEntity()
            {
                Instant = Instant.FromDateTimeOffset(date)
            };
            var tEntity = (ITableEntity)entity;

            var result = tEntity.WriteEntity(new OperationContext());

            result.Should().ContainKey(nameof(InstantEntity.Instant));
            result[nameof(InstantEntity.Instant)].DateTimeOffsetValue.Should().NotBeNull();
            result[nameof(InstantEntity.Instant)].DateTimeOffsetValue.Should().Be(date);
        }

        [Fact]
        public void Should_ReadOffsetDateTimeEntity()
        {
            var date = DateTimeOffset.Now;
            var entity = new OffsetDateTimeEntity();
            var tEntity = (ITableEntity)entity;

            tEntity.ReadEntity(new Dictionary<string, EntityProperty>
            {
                {
                    nameof(OffsetDateTimeEntity.OffsetDateTime),
                    EntityProperty.GeneratePropertyForString(OffsetDateTimePattern.Rfc3339.Format(OffsetDateTime.FromDateTimeOffset(date)))
                }
            }, new OperationContext());

            entity.OffsetDateTime.Should().Be(OffsetDateTime.FromDateTimeOffset(date));
        }

        [Fact]
        public void Should_ReadOffsetDateTimeEntity_FromDateTimeOffset()
        {
            var date = DateTimeOffset.Now;
            var entity = new OffsetDateTimeEntity();
            var tEntity = (ITableEntity)entity;

            tEntity.ReadEntity(new Dictionary<string, EntityProperty>
            {
                {
                    nameof(OffsetDateTimeEntity.OffsetDateTime),
                    EntityProperty.GeneratePropertyForDateTimeOffset(date)
                }
            }, new OperationContext());

            entity.OffsetDateTime.Should().Be(OffsetDateTime.FromDateTimeOffset(date.UtcDateTime));
        }

        [Fact]
        public void Should_WriteOffsetDateTimeEntity()
        {
            var date = DateTimeOffset.Now;
            var entity = new OffsetDateTimeEntity()
            {
                OffsetDateTime = OffsetDateTime.FromDateTimeOffset(date)
            };
            var tEntity = (ITableEntity)entity;

            var result = tEntity.WriteEntity(new OperationContext());

            result.Should().ContainKey(nameof(OffsetDateTimeEntity.OffsetDateTime));
            result[nameof(OffsetDateTimeEntity.OffsetDateTime)].DateTimeOffsetValue.Should().NotBeNull();
            result[nameof(OffsetDateTimeEntity.OffsetDateTime)].DateTimeOffsetValue.Should().Be(date);
        }

        [Fact]
        public void Should_ReadZonedDateTimeEntity()
        {
            var date = DateTimeOffset.Now;
            var entity = new ZonedDateTimeEntity();
            var tEntity = (ITableEntity)entity;

            tEntity.ReadEntity(new Dictionary<string, EntityProperty>
            {
                {
                    nameof(ZonedDateTimeEntity.ZonedDateTime),
                    EntityProperty.GeneratePropertyForString(CustomNodaConverters.ZonedDateTimeFormatter.Format(ZonedDateTime.FromDateTimeOffset(date)))
                }
            }, new OperationContext());

            entity.ZonedDateTime.Should().Be(ZonedDateTime.FromDateTimeOffset(date));
        }

        [Fact(Skip = "Not working correctly (not a high priority)")]
        public void Should_ReadZonedDateTimeEntity_FromDateTimeOffset()
        {
            var date = DateTimeOffset.Now;
            var entity = new ZonedDateTimeEntity();
            var tEntity = (ITableEntity)entity;

            tEntity.ReadEntity(new Dictionary<string, EntityProperty>
            {
                {
                    nameof(ZonedDateTimeEntity.ZonedDateTime),
                    EntityProperty.GeneratePropertyForDateTimeOffset(date)
                }
            }, new OperationContext());

            // DateTimeOffset sets to utc time under the covers
            entity.ZonedDateTime.Should().Be(ZonedDateTime.FromDateTimeOffset(date.UtcDateTime));
        }

        [Fact]
        public void Should_WriteZonedDateTimeEntity()
        {
            var date = DateTimeOffset.Now;
            var entity = new ZonedDateTimeEntity()
            {
                ZonedDateTime = ZonedDateTime.FromDateTimeOffset(date)
            };
            var tEntity = (ITableEntity)entity;

            var result = tEntity.WriteEntity(new OperationContext());

            result.Should().ContainKey(nameof(ZonedDateTimeEntity.ZonedDateTime));
            result[nameof(ZonedDateTimeEntity.ZonedDateTime)].DateTimeOffsetValue.Should().NotBeNull();
            result[nameof(ZonedDateTimeEntity.ZonedDateTime)].DateTimeOffsetValue.Should().Be(date);
        }

        [Fact]
        public void Should_ReadLocalDateEntity()
        {
            var date = DateTime.Now;
            var entity = new LocalDateEntity();
            var tEntity = (ITableEntity)entity;

            tEntity.ReadEntity(new Dictionary<string, EntityProperty>
            {
                {
                    nameof(LocalDateEntity.LocalDate),
                    EntityProperty.GeneratePropertyForString(LocalDatePattern.Iso.Format(LocalDate.FromDateTime(date)))
                }
            }, new OperationContext());

            entity.LocalDate.Should().BeLessOrEqualTo(LocalDate.FromDateTime(date));
        }

        [Fact]
        public void Should_ReadLocalDateEntity_FromDateTimeOffset()
        {
            var date = DateTime.Now;
            var entity = new LocalDateEntity();
            var tEntity = (ITableEntity)entity;

            tEntity.ReadEntity(new Dictionary<string, EntityProperty>
            {
                {
                    nameof(LocalDateEntity.LocalDate),
                    EntityProperty.GeneratePropertyForDateTimeOffset(date)
                }
            }, new OperationContext());

            entity.LocalDate.Should().BeLessOrEqualTo(LocalDate.FromDateTime(date.ToUniversalTime()));
        }

        [Fact]
        public void Should_WriteLocalDateEntity()
        {
            var date = DateTime.Today;
            var entity = new LocalDateEntity()
            {
                LocalDate = LocalDate.FromDateTime(date)
            };
            var tEntity = (ITableEntity)entity;

            var result = tEntity.WriteEntity(new OperationContext());

            result.Should().ContainKey(nameof(LocalDateEntity.LocalDate));
            result[nameof(LocalDateEntity.LocalDate)].DateTimeOffsetValue.Should().NotBeNull();
            result[nameof(LocalDateEntity.LocalDate)].DateTimeOffsetValue.Should().Be(date);
        }

        [Fact]
        public void Should_ReadLocalDateTimeEntity()
        {
            var date = DateTime.Now;
            var entity = new LocalDateTimeEntity();
            var tEntity = (ITableEntity)entity;

            tEntity.ReadEntity(new Dictionary<string, EntityProperty>
            {
                {
                    nameof(LocalDateTimeEntity.LocalDateTime),
                    EntityProperty.GeneratePropertyForString(LocalDateTimePattern.ExtendedIso.Format(LocalDateTime.FromDateTime(date)))
                }
            }, new OperationContext());

            entity.LocalDateTime.Should().BeLessOrEqualTo(LocalDateTime.FromDateTime(date));
        }

        [Fact]
        public void Should_ReadLocalDateTimeEntity_FromDateTimeOffset()
        {
            var date = DateTime.Now;
            var entity = new LocalDateTimeEntity();
            var tEntity = (ITableEntity)entity;

            tEntity.ReadEntity(new Dictionary<string, EntityProperty>
            {
                {
                    nameof(LocalDateTimeEntity.LocalDateTime),
                    EntityProperty.GeneratePropertyForDateTimeOffset(date)
                }
            }, new OperationContext());

            entity.LocalDateTime.Should().BeLessOrEqualTo(LocalDateTime.FromDateTime(date.ToUniversalTime()));
        }

        [Fact]
        public void Should_WriteLocalDateTimeEntity()
        {
            var date = DateTime.Now;
            var entity = new LocalDateTimeEntity()
            {
                LocalDateTime = LocalDateTime.FromDateTime(date)
            };
            var tEntity = (ITableEntity)entity;

            var result = tEntity.WriteEntity(new OperationContext());

            result.Should().ContainKey(nameof(LocalDateTimeEntity.LocalDateTime));
            result[nameof(LocalDateTimeEntity.LocalDateTime)].DateTimeOffsetValue.Should().NotBeNull();
            result[nameof(LocalDateTimeEntity.LocalDateTime)].DateTimeOffsetValue.Should().Be(date);
        }
    }
}
