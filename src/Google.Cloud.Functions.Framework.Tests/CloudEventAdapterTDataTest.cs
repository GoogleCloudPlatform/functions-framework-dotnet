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
using Google.Cloud.Functions.Framework.Tests.GcfEvents;
using Google.Events;
using Google.Events.Protobuf.Cloud.Storage.V1;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Google.Cloud.Functions.Framework.Tests
{
    public class CloudEventAdapterTDataTest
    {
        [Fact]
        public async Task InvalidRequest_FunctionNotCalled()
        {
            var function = FunctionForDelegate((CloudEvent cloudEvent, StorageObjectData data) => Task.CompletedTask);
            var adapter = CreateAdapter(function);
            var context = new DefaultHttpContext();
            await adapter.HandleAsync(context);
            Assert.Equal(400, context.Response.StatusCode);
            Assert.False(function.Executed);
        }

        [Fact]
        public async Task ValidRequest_FunctionCalled()
        {
            string eventId = Guid.NewGuid().ToString();
            var function = FunctionForDelegate((CloudEvent cloudEvent, StorageObjectData data) =>
            {
                Assert.Equal(eventId, cloudEvent.Id);
                Assert.Equal("TestName", data.Name);
                return Task.CompletedTask;
            });
            var adapter = CreateAdapter(function);
            var context = new DefaultHttpContext
            {
                Request =
                {
                    ContentType = "application/json",
                    Body = new MemoryStream(Encoding.UTF8.GetBytes("{\"name\":\"TestName\"}")),
                    Headers =
                    {
                        { "ce-specversion", "1.0" },
                        { "ce-source", "test" },
                        { "ce-type", "test" },
                        { "ce-id", eventId }
                    }
                }
            };
            await adapter.HandleAsync(context);
            Assert.Equal(200, context.Response.StatusCode);
            Assert.True(function.Executed);
        }

        [Fact]
        public async Task StorageEventConversion()
        {
            var (cloudEvent, data) = await DeserializeViaFunction<StorageObjectData>("storage.json");
            Assert.Equal("1147091835525187", cloudEvent.Id);

            Assert.Equal("some-bucket", data.Bucket);
            Assert.Equal(new DateTimeOffset(2020, 4, 23, 7, 38, 57, 230, TimeSpan.Zero).ToTimestamp(), data.TimeCreated);
            Assert.Equal(1587627537231057L, data.Generation);
            Assert.Equal(1L, data.Metageneration);
            Assert.Equal(352L, data.Size);
        }

        private static async Task<(CloudEvent, TData)> DeserializeViaFunction<TData>(string resource) where TData : class
        {
            (CloudEvent, TData)? ret = null;
            var function = FunctionForDelegate((CloudEvent cloudEvent, TData data) =>
            {
                ret = (cloudEvent, data);
                return Task.CompletedTask;
            });
            var adapter = CreateAdapter(function);
            var httpContext = GcfEventResources.CreateHttpContext(resource);
            await adapter.HandleAsync(httpContext);
            Assert.True(function.Executed);
            Assert.NotNull(ret);
            return ret!.Value;
        }

        private static DelegateAdapter<TData> FunctionForDelegate<TData>(Func<CloudEvent, TData, Task> func) where TData : class =>
            new DelegateAdapter<TData>(func);

        private class DelegateAdapter<TData> : ICloudEventFunction<TData> where TData : class
        {
            public bool Executed { get; private set; }
            private readonly Func<CloudEvent, TData, Task> _func;

            public DelegateAdapter(Func<CloudEvent, TData, Task> func) =>
                _func = func;

            public async Task HandleAsync(CloudEvent cloudEvent, TData data, CancellationToken cancellationToken)
            {
                await _func(cloudEvent, data);
                Executed = true;
            }
        }

        private static CloudEventAdapter<TData> CreateAdapter<TData>(ICloudEventFunction<TData> function) where TData : class =>
            new CloudEventAdapter<TData>(function, CloudEventFormatterAttribute.CreateFormatter(typeof(TData))!, new NullLogger<CloudEventAdapter<TData>>());
    }
}
