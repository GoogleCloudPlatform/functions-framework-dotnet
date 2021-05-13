// Copyright 2021, Google LLC
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
using CloudNative.CloudEvents.SystemTextJson;
using Google.Cloud.Functions.Framework;
using Google.Cloud.Functions.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Google.Cloud.Functions.Hosting.Tests
{
    public class CustomCloudEventDataFunction : ICloudEventFunction<CustomCloudEventData>
    {
        private readonly ILogger _logger;

        public CustomCloudEventDataFunction(ILogger<CustomCloudEventDataFunction> logger) =>
            _logger = logger;

        public Task HandleAsync(CloudEvent cloudEvent, CustomCloudEventData data, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Function called, Value={value}", data.Value);
            return Task.CompletedTask;
        }
    }

    public class CustomCloudEventData
    {
        [JsonProperty("xyz")]
        public string Value { get; set; }

        public static CloudEvent CreateSampleEvent() =>
            new CloudEvent
            {
                Id = "event-id",
                Type = "event-type",
                Source = new Uri("//sampleevent", UriKind.RelativeOrAbsolute),
                Data = new CustomCloudEventData { Value = "abc" }
            };
    }

    public abstract class CustomCloudEventDataTestBase : FunctionTestBase<CustomCloudEventDataFunction>
    {
        protected static CloudEventFormatter Formatter { get; } = new JsonEventFormatter<CustomCloudEventData>();

        public Task ExecuteAsync()
        {
            var cloudEvent = new CloudEvent
            {
                Id = "event-id",
                Type = "event-type",
                Source = new Uri("//sampleevent", UriKind.RelativeOrAbsolute),
                Data = new CustomCloudEventData { Value = "abc" }
            };
            return ExecuteCloudEventRequestAsync(cloudEvent, Formatter);
        }
    }

    public class CustomCloudEventDataTest_NoFormatter : CustomCloudEventDataTestBase
    {
        [Fact]
        public async Task RequestFails()
        {
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ExecuteAsync());
            // Check it's our custom message, in a fairly loose way.
            Assert.Contains("Functions Framework", exception.Message);
        }
    }

    [FunctionsStartup(typeof(Startup))]
    public class CustomCloudEventDataTest_WithStartup : CustomCloudEventDataTestBase
    {
        [Fact]
        public async Task RequestSucceeds()
        {
            await ExecuteAsync();
            var log = Assert.Single(GetFunctionLogEntries());
            Assert.Equal("Function called, Value=abc", log.Message);
        }

        private class Startup : FunctionsStartup
        {
            public override void ConfigureServices(WebHostBuilderContext context, IServiceCollection services) =>
                services.AddSingleton(Formatter);
        }
    }
}
