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
using Google.Cloud.Functions.Framework;
using Google.Cloud.Functions.Invoker.Testing;
using Google.Events;
using Google.Events.Protobuf.Cloud.Storage.V1;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Google.Cloud.Functions.Testing.Tests
{
    public class FunctionTestBaseTest
    {
        [Fact]
        public async Task ExecuteCloudEventAsync_CloudEvent()
        {
            var cloudEvent = new CloudEvent(
                StorageObjectData.FinalizedCloudEventType,
                new Uri("//storage", UriKind.RelativeOrAbsolute));
            var data = new StorageObjectData { Name = "test1" };
            CloudEventConverters.PopulateCloudEvent(cloudEvent, data);

            using var test = new Test<StorageObjectFunction>();
            await test.ExecuteCloudEventRequestAsync(cloudEvent);
            var log = Assert.Single(test.GetFunctionLogEntries());
            Assert.Equal("Name: test1", log.Message);
        }

        [Fact]
        public async Task ExecuteCloudEventAsync_Components()
        {
            using var test = new Test<StorageObjectFunction>();
            await test.ExecuteCloudEventRequestAsync(
                StorageObjectData.FinalizedCloudEventType,
                new StorageObjectData { Name = "test2" });
            var log = Assert.Single(test.GetFunctionLogEntries());
            Assert.Equal("Name: test2", log.Message);
        }

        // FunctionTestBase is abstract; this class just allows us to test multiple functions
        // easily in one class.
        private class Test<TFunction> : FunctionTestBase<TFunction>
        {
        }

        private class StorageObjectFunction : ICloudEventFunction<StorageObjectData>
        {
            private ILogger _logger;

            public StorageObjectFunction(ILogger<StorageObjectFunction> logger) =>
                _logger = logger;

            public Task HandleAsync(CloudEvent cloudEvent, StorageObjectData data, CancellationToken cancellationToken)
            {
                _logger.LogInformation($"Name: {data.Name}");
                return Task.CompletedTask;
            }
        }
    }
}
