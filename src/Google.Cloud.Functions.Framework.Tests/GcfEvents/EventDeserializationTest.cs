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

using Google.Cloud.Functions.Framework.GcfEvents;
using Google.Events;
using Google.Events.SystemTextJson.Cloud.PubSub.V1;
using Google.Events.SystemTextJson.Cloud.Storage.V1;
using Google.Events.SystemTextJson.Firebase.V1;
using Google.Events.SystemTextJson.Type;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using FirestoreDocumentEventData = Google.Events.SystemTextJson.Cloud.Firestore.V1.DocumentEventData;

namespace Google.Cloud.Functions.Framework.Tests.GcfEvents
{
    public class EventDeserializationTest
    {
        [Fact]
        public async Task Storage()
        {
            var data = await ConvertAndDeserialize<StorageObjectData>("storage.json");

            Assert.Equal("some-bucket", data.Bucket);
            Assert.Equal(new DateTimeOffset(2020, 4, 23, 7, 38, 57, 230, TimeSpan.Zero), data.TimeCreated);
            Assert.Equal(1587627537231057L, data.Generation);
            Assert.Equal(1L, data.Metageneration);
            Assert.Equal(352UL, data.Size);
        }

        [Fact]
        public async Task Firestore_Simple()
        {
            var data = await ConvertAndDeserialize<FirestoreDocumentEventData>("firestore_simple.json");

            Assert.Equal("2Vm2mI1d0wIaK2Waj5to", data.Wildcards["doc"]);
            Assert.Equal("projects/project-id/databases/(default)/documents/gcf-test/2Vm2mI1d0wIaK2Waj5to", data.Value!.Name);
            Assert.Equal(new DateTimeOffset(2020, 4, 23, 9, 58, 53, 211, TimeSpan.Zero).AddTicks(350), data.Value.CreateTime);
            Assert.Equal(new DateTimeOffset(2020, 4, 23, 12, 0, 27, 247, TimeSpan.Zero).AddTicks(1870), data.Value.UpdateTime);
            IDictionary<string, object?> fields = data.Value!.Fields!;
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
            IDictionary<string, object?> fields = data.Value!.Fields!;

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
        public async Task PubSub_Text()
        {
            var data = await ConvertAndDeserialize<MessagePublishedData>("pubsub_text.json");
            var message = data.Message!;
            Assert.NotNull(message);
            Assert.Equal("test message 3", message.TextData);
            Assert.Equal(1, message.Attributes.Count);
            Assert.Equal("attr1-value", message.Attributes["attr1"]);
        }

        [Fact]
        public async Task PubSub_Binary()
        {
            var data = await ConvertAndDeserialize<MessagePublishedData>("pubsub_binary.json");
            var message = data.Message!;
            Assert.NotNull(message);
            Assert.Equal(new byte[] { 1, 2, 3, 4 }, message.Data);
            Assert.Equal(0, message.Attributes.Count);
        }

        [Fact]
        public async Task PubSub_Attributes()
        {
            var data = await ConvertAndDeserialize<MessagePublishedData>("pubsub_text.json");
            var message = data.Message!;
            Assert.NotNull(message);
            Assert.Equal(1, message.Attributes.Count);
            Assert.Equal("attr1-value", message.Attributes["attr1"]);

            data = await ConvertAndDeserialize<MessagePublishedData>("pubsub_binary.json");
            message = data.Message!;
            Assert.Equal(0, message.Attributes.Count);
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
            ConvertAndDeserialize<DocumentEventData>(resource);

        [Fact]
        public async Task FirebaseDatabaseEvent_NullData()
        {
            var firebaseEvent = await ConvertAndDeserialize<DocumentEventData>("firebase-db1.json");
            Assert.Null(firebaseEvent.Data);
        }

        [Fact]
        public async Task FirebaseDatabaseEvent_Wildcards()
        {
            var firebaseEvent = await ConvertAndDeserialize<DocumentEventData>("firebase-db1.json");
            Assert.Equal("xyz", firebaseEvent.Wildcards["child"]);
        }

        [Fact]
        public async Task FirebaseDatabaseEvent_ObjectData()
        {
            var firebaseEvent = await ConvertAndDeserialize<DocumentEventData>("firebase-db2.json");
            Assert.NotNull(firebaseEvent.Data);
            var dataElement = firebaseEvent.Data!.Value;
            Assert.Equal(JsonValueKind.Object, dataElement.ValueKind);
            Assert.Equal("other", dataElement.GetProperty("grandchild").GetString());

            Assert.NotNull(firebaseEvent.Delta);
            var deltaElement = firebaseEvent.Delta!.Value;
            Assert.Equal(JsonValueKind.Object, deltaElement.ValueKind);
            Assert.Equal("other changed", deltaElement.GetProperty("grandchild").GetString());
        }

        [Fact]
        public async Task FirebaseDatabaseEvent_NumericData()
        {
            var firebaseEvent = await ConvertAndDeserialize<DocumentEventData>("firebase-dbdelete2.json");
            Assert.NotNull(firebaseEvent.Data);
            var element = firebaseEvent.Data!.Value;
            Assert.Equal(JsonValueKind.Number, element.ValueKind);
            Assert.Equal(10, element.GetInt32());
        }

        private static async Task<T> ConvertAndDeserialize<T>(string resourceName) where T : class
        {
            var context = GcfEventResources.CreateHttpContext(resourceName);
            var cloudEvent = await GcfConverters.ConvertGcfEventToCloudEvent(context.Request);
            return CloudEventConverters.ConvertCloudEventData<T>(cloudEvent);
        }
    }
}
