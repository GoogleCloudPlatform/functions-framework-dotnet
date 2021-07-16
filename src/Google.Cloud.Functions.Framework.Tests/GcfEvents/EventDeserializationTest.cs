// Copyright 2020, Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using CloudNative.CloudEvents;
using Google.Cloud.Functions.Framework.GcfEvents;
using Google.Events;
using Google.Events.Protobuf;
using Google.Events.Protobuf.Cloud.PubSub.V1;
using Google.Events.Protobuf.Cloud.Storage.V1;
using Google.Events.Protobuf.Firebase.Analytics.V1;
using Google.Events.Protobuf.Firebase.Auth.V1;
using Google.Events.Protobuf.Firebase.Database.V1;
using Google.Protobuf.WellKnownTypes;
using Google.Type;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using FirestoreDocumentEventData = Google.Events.Protobuf.Cloud.Firestore.V1.DocumentEventData;

namespace Google.Cloud.Functions.Framework.Tests.GcfEvents
{
    public class EventDeserializationTest
    {
        [Fact]
        public async Task Storage()
        {
            var data = await ConvertAndDeserialize<StorageObjectData>("storage.json");

            Assert.Equal("some-bucket", data.Bucket);
            Assert.Equal(new DateTimeOffset(2020, 4, 23, 7, 38, 57, 230, TimeSpan.Zero).ToTimestamp(), data.TimeCreated);
            Assert.Equal(1587627537231057L, data.Generation);
            Assert.Equal(1L, data.Metageneration);
            Assert.Equal(352L, data.Size);
        }

        [Fact]
        public async Task Firestore_Simple()
        {
            var data = await ConvertAndDeserialize<FirestoreDocumentEventData>("firestore_simple.json");

            Assert.Equal("projects/project-id/databases/(default)/documents/gcf-test/2Vm2mI1d0wIaK2Waj5to", data.Value!.Name);
            Assert.Equal(new DateTimeOffset(2020, 4, 23, 9, 58, 53, 211, TimeSpan.Zero).AddTicks(350).ToTimestamp(), data.Value.CreateTime);
            Assert.Equal(new DateTimeOffset(2020, 4, 23, 12, 0, 27, 247, TimeSpan.Zero).AddTicks(1870).ToTimestamp(), data.Value.UpdateTime);
            IReadOnlyDictionary<string, object?> fields = data.Value!.ConvertFields();
            Assert.Equal(3, fields.Count);
            Assert.Equal("asd", fields["another test"]);
            Assert.Equal(4L, fields["count"]);
            Assert.Equal("bar", fields["foo"]);

            Assert.Equal(new List<string> { "count" }, data.UpdateMask!.FieldPaths);

            // Just check one aspect of OldValue, to make sure it's there...
            Assert.Equal("projects/project-id/databases/(default)/documents/gcf-test/2Vm2mI1d0wIaK2Waj5to", data.OldValue!.Name);
        }

        [Fact]
        public async Task Firestore_Complex()
        {
            var data = await ConvertAndDeserialize<FirestoreDocumentEventData>("firestore_complex.json");
            IReadOnlyDictionary<string, object?> fields = data.Value!.ConvertFields();

            Assert.Equal(10, fields.Count);
            Assert.Equal(new object[] { 1L, 2L }, fields["arrayValue"]);
            Assert.Equal(true, fields["booleanValue"]);
            Assert.Equal(51.4543, ((LatLng) fields["geoPointValue"]!).Latitude);
            Assert.Equal(-0.9781, ((LatLng) fields["geoPointValue"]!).Longitude);
            Assert.Equal(50L, fields["intValue"]);
            Assert.Equal(5.5, fields["doubleValue"]);
            Assert.Null(fields["nullValue"]);
            Assert.Equal("projects/project-id/databases/(default)/documents/foo/bar/baz/qux", fields["referenceValue"]);
            Assert.Equal("text", fields["stringValue"]);
            Assert.Equal(new DateTimeOffset(2020, 4, 23, 14, 23, 53, 241, TimeSpan.Zero), fields["timestampValue"]);

            var map = (IDictionary<string, object?>) fields["mapValue"]!;
            Assert.Equal(2, map.Count);
            Assert.Equal("x", map["field1"]);
            Assert.Equal(new object[] { "x", 1L }, map["field2"]);
        }

        [Fact]
        public async Task PubSub_SyntheticData()
        {
            var data = await ConvertAndDeserialize<MessagePublishedData>("pubsub_text.json");
            var message = data.Message!;
            Assert.NotNull(message);
            var expectedTimestamp = Timestamp.FromDateTimeOffset(new DateTimeOffset(2020, 5, 6, 7, 33, 34, 556, TimeSpan.Zero));
            Assert.Equal(expectedTimestamp, message.PublishTime);
            Assert.Equal("1144231683168617", message.MessageId);
        }

        [Fact]
        public async Task PubSub_Text()
        {
            var data = await ConvertAndDeserialize<MessagePublishedData>("pubsub_text.json");
            var message = data.Message!;
            Assert.NotNull(message);
            Assert.Equal("test message 3", message.TextData);
        }

        [Fact]
        public async Task PubSub_Binary()
        {
            var data = await ConvertAndDeserialize<MessagePublishedData>("pubsub_binary.json");
            var message = data.Message!;
            Assert.NotNull(message);
            Assert.Equal(new byte[] { 1, 2, 3, 4 }, message.Data);
        }

        [Fact]
        public async Task PubSub_Attributes()
        {
            var data = await ConvertAndDeserialize<MessagePublishedData>("pubsub_text.json");
            var message = data.Message!;
            Assert.NotNull(message);
            Assert.Equal(new Dictionary<string, string> { { "attr1", "attr1-value" } }, message.Attributes);

            data = await ConvertAndDeserialize<MessagePublishedData>("pubsub_binary.json");
            message = data.Message!;
            Assert.Empty(message.Attributes);
        }

        [Fact]
        public async Task LegacyEvents()
        {
            // Just a quick validation that the legacy events are in the same format.
            var data = await ConvertAndDeserialize<MessagePublishedData>("legacy_pubsub.json");
            var message = data.Message!;
            Assert.Equal("This is a sample message", message.TextData);
            var storageEvent = await ConvertAndDeserialize<StorageObjectData>("legacy_storage_change.json");
            Assert.Equal("sample-bucket", storageEvent.Bucket);
        }

        [Theory]
        [InlineData("firebase-db1.json")]
        [InlineData("firebase-db2.json")]
        [InlineData("firebase-db3.json")]
        [InlineData("firebase-db4.json")]
        [InlineData("firebase-db5.json")]
        [InlineData("firebase-db6.json")]
        [InlineData("firebase-db7.json")]
        [InlineData("firebase-db8.json")]
        [InlineData("firebase-dbdelete1.json")]
        [InlineData("firebase-dbdelete2.json")]
        public Task FirebaseDatabaseEvent_ParseWithoutError(string resource) =>
            ConvertAndDeserialize<ReferenceEventData>(resource);

        [Fact]
        public async Task FirebaseDatabaseEvent_NullData()
        {
            var firebaseEvent = await ConvertAndDeserialize<ReferenceEventData>("firebase-db1.json");
            Assert.Equal(Value.KindOneofCase.NullValue, firebaseEvent.Data.KindCase);
        }

        [Fact]
        public async Task FirebaseDatabaseEvent_ObjectData()
        {
            var firebaseEvent = await ConvertAndDeserialize<ReferenceEventData>("firebase-db2.json");
            Assert.NotNull(firebaseEvent.Data);
            var data = firebaseEvent.Data;
            Assert.Equal(Value.KindOneofCase.StructValue, data.KindCase);
            Assert.Equal("other", data.StructValue.GetValue("grandchild"));

            Assert.NotNull(firebaseEvent.Delta);
            var delta = firebaseEvent.Delta;
            Assert.Equal(Value.KindOneofCase.StructValue, delta.KindCase);
            Assert.Equal("other changed", delta.StructValue.GetValue("grandchild"));
        }

        [Fact]
        public async Task FirebaseDatabaseEvent_NumericData()
        {
            var firebaseEvent = await ConvertAndDeserialize<ReferenceEventData>("firebase-dbdelete2.json");
            Assert.NotNull(firebaseEvent.Data);
            var data = firebaseEvent.Data;
            Assert.Equal(Value.KindOneofCase.NumberValue, data.KindCase);
            Assert.Equal(10, data.NumberValue);
        }

        [Fact]
        public async Task FirebaseAnalytics()
        {
            var firebaseEvent = await ConvertAndDeserialize<AnalyticsLogData>("firebase-analytics.json");
            // Test a small subset of the data
            var eventDim = Assert.Single(firebaseEvent.EventDim);
            Assert.Equal(1606955191246909L, eventDim.TimestampMicros);
            Assert.Equal(1606951997533000, eventDim.PreviousTimestampMicros);

            var userDim = firebaseEvent.UserDim;
            Assert.Equal("aaabbb11122233344455566677788899", userDim.AppInfo.AppInstanceId);
            Assert.Equal("ANDROID", userDim.AppInfo.AppPlatform);

            Assert.Equal("Galaxy A30s", userDim.DeviceInfo.MobileMarketingName);

            Assert.Equal(4, userDim.UserProperties.Count);
            Assert.Equal("true", userDim.UserProperties["completed_tutorial"].Value.StringValue);
        }

        [Fact]
        public async Task FirebaseAuth_MetadataNamesAdjusted()
        {
            var authData = await ConvertAndDeserialize<AuthEventData>("firebase-auth1.json");
            Assert.Equal(new System.DateTime(2020, 5, 26, 10, 42, 27), authData.Metadata.CreateTime.ToDateTime());
            Assert.Equal(new System.DateTime(2020, 5, 29, 11, 00, 00), authData.Metadata.LastSignInTime.ToDateTime());
        }

        private static async Task<T> ConvertAndDeserialize<T>(string resourceName) where T : class
        {
            var context = GcfEventResources.CreateHttpContext(resourceName);
            var formatter = CloudEventFormatterAttribute.CreateFormatter(typeof(T))
                ?? throw new InvalidOperationException("No formatter available");
            var cloudEvent = await GcfConverters.ConvertGcfEventToCloudEvent(context.Request, formatter);
            // We know that ConvertGcfEventToCloudEvent always populates the Data property.
            return (T) cloudEvent.Data!;
        }
    }
}
