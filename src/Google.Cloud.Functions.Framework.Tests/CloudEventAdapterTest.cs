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
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Google.Cloud.Functions.Framework.Tests
{
    public class CloudEventAdapterTest
    {
        [Fact]
        public async Task InvalidRequest_FunctionNotCalled()
        {
            var function = new TestCloudEventFunction();
            var adapter = new CloudEventAdapter(function, new NullLogger<CloudEventAdapter>());
            var context = new DefaultHttpContext();
            await adapter.HandleAsync(context);
            Assert.Equal(400, context.Response.StatusCode);
            Assert.Null(function.LastEvent);
        }

        [Fact]
        public async Task ValidRequest_FunctionCalled()
        {
            var function = new TestCloudEventFunction();
            var adapter = new CloudEventAdapter(function, new NullLogger<CloudEventAdapter>());
            string eventId = Guid.NewGuid().ToString();
            var context = new DefaultHttpContext
            {
                Request =
                {
                    // No actual content, but that's okay.
                    ContentType = "application/json",
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
            Assert.Equal(eventId, function.LastEvent?.Id);
        }

        private class TestCloudEventFunction : ICloudEventFunction
        {
            public CloudEvent? LastEvent { get; private set; }

            public Task HandleAsync(CloudEvent cloudEvent, CancellationToken cancellationToken)
            {
                LastEvent = cloudEvent;
                return Task.CompletedTask;
            }
        }
    }
}
