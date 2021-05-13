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
using CloudNative.CloudEvents.NewtonsoftJson;
using Google.Cloud.Functions.Framework;
using Google.Cloud.Functions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Google.Cloud.Functions.Examples.CustomEventDataFunction
{
    /// <summary>
    /// Startup class to inject a suitable CloudEventFormatter. Our CustomData type
    /// has attributes for Json.NET, but no CloudEventFormatterAttribute, so we inject
    /// a suitable JsonEventFormatter.
    /// </summary>
    public class Startup : FunctionsStartup
    {
        public override void ConfigureServices(WebHostBuilderContext context, IServiceCollection services) =>
            services.AddSingleton<CloudEventFormatter>(new JsonEventFormatter<CustomData>());
    }

    /// <summary>
    /// A function that can be triggered by a CloudEvent containing data of type
    /// CustomData in a JSON event format.
    /// </summary>
    [FunctionsStartup(typeof(Startup))]
    public class Function : ICloudEventFunction<CustomData>
    {
        private readonly ILogger _logger;

        public Function(ILogger<Function> logger) =>
            _logger = logger;

        public Task HandleAsync(CloudEvent cloudEvent, CustomData data, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Data received. TextValue={value}", data.TextValue);
            return Task.CompletedTask;
        }
    }

    public class CustomData
    {
        [JsonProperty("text")]
        public string TextValue { get; set; }
    }
}
