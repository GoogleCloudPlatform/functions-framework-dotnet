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

using Google.Cloud.Functions.Framework.LegacyEvents;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Google.Cloud.Functions.Framework.Tests.LegacyEvents
{
    public class LegacyEventAdapterTest
    {
        /// <summary>
        /// Test the overall flow, including context and data deserialization, and function execution.
        /// Most other tests focus on one aspect, e.g. data deserialization.
        /// </summary>
        [Fact]
        public async Task SmokeTest()
        {
            bool executed = false;
            var function = FunctionForDelegate((StorageObject data, Context context) =>
            {
                Assert.Equal("1147091835525187", context.Id);
                Assert.Equal(new DateTimeOffset(2020, 4, 23, 7, 38, 57, 772, TimeSpan.Zero), context.Timestamp);
                Assert.NotNull(context.Resource);
                Assert.Equal("projects/_/buckets/some-bucket/objects/Test.cs", context.Resource!.Name);
                Assert.Equal("some-bucket", data.Bucket);

                executed = true;
                return Task.CompletedTask;
            });
            var adapter = CreateAdapter(function);

            var httpContext = CreateHttpContext("storage.json");
            await adapter.HandleAsync(httpContext);
            Assert.True(executed);
        }

        [Fact]
        public async Task ContextMerging()
        {
            // The Firestore events have context data at the root of the request.
            bool executed = false;
            var function = FunctionForDelegate((FirestoreEvent data, Context context) =>
            {
                Assert.Equal("projects/project-id/databases/(default)/documents/gcf-test/2Vm2mI1d0wIaK2Waj5to", context.Resource.Name);
                Assert.Null(context.Resource.Service);
                Assert.Null(context.Resource.Type);

                Assert.Equal("7b8f1804-d38b-4b68-b37d-e2fb5d12d5a0-0", context.Id);
                Assert.Equal(new DateTimeOffset(2020, 4, 23, 12, 0, 27, 247, TimeSpan.Zero).AddTicks(1870), context.Timestamp);
                Assert.Equal("providers/cloud.firestore/eventTypes/document.write", context.Type);

                executed = true;
                return Task.CompletedTask;
            });
            var adapter = CreateAdapter(function);

            var httpContext = CreateHttpContext("firestore_simple.json");
            await adapter.HandleAsync(httpContext);
            Assert.True(executed);
        }

        [Fact]
        public async Task StorageDeserialization()
        {
            var data = await DeserializeViaFunction<StorageObject>("storage.json");
            Assert.Equal("some-bucket", data.Bucket);
            Assert.Equal(new DateTimeOffset(2020, 4, 23, 7, 38, 57, 230, TimeSpan.Zero), data.TimeCreated);
            Assert.Equal(1587627537231057L, data.Generation);
            Assert.Equal(1L, data.MetaGeneration);
            Assert.Equal(352UL, data.Size);
        }

        [Fact]
        public async Task PubSubDeserialization_Text()
        {
            var data = await DeserializeViaFunction<PubSubMessage>("pubsub_text.json");
            Assert.Equal("test message 3", data.TextData);
        }

        [Fact]
        public async Task PubSubDeserialization_Binary()
        {
            var data = await DeserializeViaFunction<PubSubMessage>("pubsub_binary.json");
            Assert.Equal(new byte[] { 1, 2, 3, 4 }, data.Data);
        }

        [Fact]
        public async Task PubSubDeserialization_Attributes()
        {
            // The text message has an attribute; the binary message doesn't.
            var data = await DeserializeViaFunction<PubSubMessage>("pubsub_text.json");
            Assert.Equal(1, data.Attributes.Count);
            Assert.Equal("attr1-value", data.Attributes["attr1"]);

            data = await DeserializeViaFunction<PubSubMessage>("pubsub_binary.json");
            Assert.Equal(0, data.Attributes.Count);
        }

        [Fact]
        public async Task Firestore_Simple()
        {
            var data = await DeserializeViaFunction<FirestoreEvent>("firestore_simple.json");

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
            var data = await DeserializeViaFunction<FirestoreEvent>("firestore_complex.json");
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

        private static async Task<T> DeserializeViaFunction<T>(string resource) where T : class
        {
            T? ret = null;
            var function = FunctionForDelegate((T data, Context context) => 
            {
                ret = data;
                return Task.CompletedTask;
            });
            var adapter = CreateAdapter(function);

            var httpContext = CreateHttpContext(resource);
            await adapter.HandleAsync(httpContext);
            Assert.NotNull(ret);
            return ret!;
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
            var function = FunctionForDelegate((StorageObject data, Context context) =>
            {
                throw new Exception("Function should not be called");
            });
            var adapter = CreateAdapter(function);
            var httpContext = new DefaultHttpContext
            {
                Request =
                {
                    Body = new MemoryStream(Encoding.UTF8.GetBytes(json)),
                    ContentType = "application/json"
                }
            };
            await adapter.HandleAsync(httpContext);
            Assert.Equal((int) HttpStatusCode.BadRequest, httpContext.Response.StatusCode);
        }

        private static ILegacyEventFunction<T> FunctionForDelegate<T>(Func<T, Context, Task> func) where T : class =>
            new DelegateAdapter<T>(func);

        private class DelegateAdapter<T> : ILegacyEventFunction<T> where T : class
        {
            private readonly Func<T, Context, Task> _func;

            public DelegateAdapter(Func<T, Context, Task> func) =>
                _func = func;

            public Task HandleAsync(T payload, Context context) => _func(payload, context);
        }

        private static LegacyEventAdapter<T> CreateAdapter<T>(ILegacyEventFunction<T> function) where T : class =>
            new LegacyEventAdapter<T>(function, new NullLogger<LegacyEventAdapter<T>>());

        private static HttpContext CreateHttpContext(string resourceName) =>
            new DefaultHttpContext
            {
                Request =
                {
                    Body = TestResourceHelper.LoadResource<LegacyEventAdapterTest>(resourceName),
                    ContentType = "application/json"
                }
            };
    }
}
