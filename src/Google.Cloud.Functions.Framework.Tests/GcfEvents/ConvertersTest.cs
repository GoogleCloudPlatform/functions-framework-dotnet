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
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Google.Cloud.Functions.Framework.Tests.GcfEvents
{
    public class ConvertersTest
    {
        [Fact]
        public async Task StorageDeserialization()
        {
            var request = CreateHttpContext("storage.json").Request;
            var cloudEvent = await GcfConverters.ConvertStorageObject(request);
            var data = CloudEventAdapter<StorageObject>.ConvertData(cloudEvent);
            Assert.Equal("1147091835525187", cloudEvent.Id);
            Assert.Equal("com.google.cloud.storage.object.finalize.v0", cloudEvent.Type);
            Assert.Equal(new DateTime(2020, 4, 23, 7, 38, 57, 772), cloudEvent.Time);
            Assert.Equal(new Uri("//storage.googleapis.com/projects/_/buckets/some-bucket/objects/Test.cs"), cloudEvent.Source);

            Assert.Equal("some-bucket", data.Bucket);
            Assert.Equal(new DateTimeOffset(2020, 4, 23, 7, 38, 57, 230, TimeSpan.Zero), data.TimeCreated);
            Assert.Equal(1587627537231057L, data.Generation);
            Assert.Equal(1L, data.MetaGeneration);
            Assert.Equal(352UL, data.Size);
        }

        [Fact]
        public async Task ContextMerging()
        {
            // The Firestore events have context data at the root of the request, along with "params" that we merge into the data.
            var request = CreateHttpContext("firestore_simple.json").Request;
            var cloudEvent = await GcfConverters.ConvertFirestoreEvent(request);
            var data = CloudEventAdapter<FirestoreEvent>.ConvertData(cloudEvent);

            Assert.Equal(new Uri("//firestore.googleapis.com/projects/project-id/databases/(default)/documents/gcf-test/2Vm2mI1d0wIaK2Waj5to"), cloudEvent.Source);
            Assert.Equal("7b8f1804-d38b-4b68-b37d-e2fb5d12d5a0-0", cloudEvent.Id);
            Assert.Equal(new DateTime(2020, 4, 23, 12, 0, 27, 247).AddTicks(1870), cloudEvent.Time);
            Assert.Equal("com.google.firestore.document.write.v0", cloudEvent.Type);

            Assert.Equal("2Vm2mI1d0wIaK2Waj5to", data.Wildcards["doc"]);
        }

        [Fact]
        public async Task PubSubDeserialization_Text()
        {
            var request = CreateHttpContext("pubsub_text.json").Request;
            var cloudEvent = await GcfConverters.ConvertPubSubMessage(request);
            Assert.Equal("com.google.cloud.pubsub.topic.publish.v0", cloudEvent.Type);
            var data = CloudEventAdapter<PubSubMessage>.ConvertData(cloudEvent);
            Assert.Equal("test message 3", data.TextData);
        }

        [Fact]
        public async Task PubSubDeserialization_Binary()
        {
            var request = CreateHttpContext("pubsub_binary.json").Request;
            var cloudEvent = await GcfConverters.ConvertPubSubMessage(request);
            var data = CloudEventAdapter<PubSubMessage>.ConvertData(cloudEvent);
            Assert.Equal(new byte[] { 1, 2, 3, 4 }, data.Data);
        }

        [Fact]
        public async Task PubSubDeserialization_Attributes()
        {
            // The text message has an attribute; the binary message doesn't.
            var request = CreateHttpContext("pubsub_text.json").Request;
            var cloudEvent = await GcfConverters.ConvertPubSubMessage(request);
            var data = CloudEventAdapter<PubSubMessage>.ConvertData(cloudEvent);
            Assert.Equal(1, data.Attributes.Count);
            Assert.Equal("attr1-value", data.Attributes["attr1"]);

            request = CreateHttpContext("pubsub_binary.json").Request;
            cloudEvent = await GcfConverters.ConvertPubSubMessage(request);
            data = CloudEventAdapter<PubSubMessage>.ConvertData(cloudEvent);
            Assert.Equal(0, data.Attributes.Count);
        }

        [Fact]
        public async Task Firestore_Simple()
        {
            var request = CreateHttpContext("firestore_simple.json").Request;
            var cloudEvent = await GcfConverters.ConvertFirestoreEvent(request);
            var data = CloudEventAdapter<FirestoreEvent>.ConvertData(cloudEvent);

            Assert.Equal("projects/project-id/databases/(default)/documents/gcf-test/2Vm2mI1d0wIaK2Waj5to", data.Value!.Name);
            Assert.Equal(new DateTimeOffset(2020, 4, 23, 9, 58, 53, 211, TimeSpan.Zero).AddTicks(350), data.Value.CreateTime);
            Assert.Equal(new DateTimeOffset(2020, 4, 23, 12, 0, 27, 247, TimeSpan.Zero).AddTicks(1870), data.Value.UpdateTime);
            IDictionary<string, object?> fields = data.Value!.Fields!;
            Assert.Equal(3, fields.Count);
            Assert.Equal("asd", fields["another test"]);
            Assert.Equal(4L, fields["count"]);
            Assert.Equal("bar", fields["foo"]);

            Assert.Equal(new List<string> { "count" }, data.UpdateMask!.FieldList);

            // Just check one aspect of OldValue, to make sure it's there...
            Assert.Equal("projects/project-id/databases/(default)/documents/gcf-test/2Vm2mI1d0wIaK2Waj5to", data.OldValue!.Name);
        }

        [Fact]
        public async Task Firestore_Complex()
        {
            // This is really all about how values are deserialized.
            var request = CreateHttpContext("firestore_complex.json").Request;
            var cloudEvent = await GcfConverters.ConvertFirestoreEvent(request);
            var data = CloudEventAdapter<FirestoreEvent>.ConvertData(cloudEvent);
            IDictionary<string, object?> fields = data.Value!.Fields!;

            Assert.Equal(10, fields.Count);
            Assert.Equal(new object[] { 1L, 2L }, fields["arrayValue"]);
            Assert.Equal(true, fields["booleanValue"]);
            Assert.Equal(51.4543, ((FirestoreGeoPoint) fields["geoPointValue"]!).Latitude);
            Assert.Equal(-0.9781, ((FirestoreGeoPoint) fields["geoPointValue"]!).Longitude);
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
        public Task InvalidRequest_UnableToDeserialize() =>
            AssertInvalidRequest("{\"context\": {}, \"data\": { invalidjson }}");

        [Fact]
        public Task InvalidRequest_NoData() =>
            AssertInvalidRequest("{\"context\": {}}");

        [Fact]
        public Task InvalidRequest_NoContext() =>
            AssertInvalidRequest("{\"data\": {}}");

        private static async Task AssertInvalidRequest(string json)
        {
            var httpContext = new DefaultHttpContext
            {
                Request =
                {
                    Body = new MemoryStream(Encoding.UTF8.GetBytes(json)),
                    ContentType = "application/json"
                }
            };
            await Assert.ThrowsAsync<CloudEventConversionException>(() => GcfConverters.ConvertStorageObject(httpContext.Request).AsTask());
        }

        internal static HttpContext CreateHttpContext(string resourceName) =>
            new DefaultHttpContext
            {
                Request =
                {
                    Body = TestResourceHelper.LoadResource<ConvertersTest>(resourceName),
                    ContentType = "application/json"
                }
            };
    }
}
