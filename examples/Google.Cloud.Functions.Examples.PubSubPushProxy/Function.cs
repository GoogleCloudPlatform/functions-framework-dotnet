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
using CloudNative.CloudEvents.Core;
using CloudNative.CloudEvents.Http;
using Google.Cloud.Functions.Framework;
using Google.Cloud.Functions.Hosting;
using Google.Events.Protobuf;
using Google.Events.Protobuf.Cloud.PubSub.V1;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Google.Cloud.Functions.Examples.PubSubPushProxy
{
    public class PubSubFunctionOptions
    {
        public const string ConfigurationSection = "PubSubFunction";

        public string Url { get; set; }
    }

    public class Startup : FunctionsStartup
    {
        public override void ConfigureServices(WebHostBuilderContext context, IServiceCollection services)
        {
            PubSubFunctionOptions options = new PubSubFunctionOptions();
            context.Configuration
                .GetSection(PubSubFunctionOptions.ConfigurationSection)
                .Bind(options);

            services.AddSingleton(options).AddHttpClient();
        }
    }

    [FunctionsStartup(typeof(Startup))]
    public class Function : IHttpFunction
    {
        private static readonly CloudEventFormatter s_formatter = new ProtobufJsonCloudEventFormatter<MessagePublishedData>();
        private readonly PubSubFunctionOptions _options;
        private readonly ILogger _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public Function(PubSubFunctionOptions options, ILogger<Function> logger, IHttpClientFactory client) =>
            (_options, _logger, _httpClientFactory) = (options, logger, client);

        public async Task HandleAsync(HttpContext context)
        {
            // Parse the incoming request. The body is actually the same as we'd use for a
            // binary mode CloudEvent, but parsing it allows us to set appropriate CloudEvent attributes.
            var memory = await BinaryDataUtilities.ToReadOnlyMemoryAsync(context.Request.Body);
            var cloudEvent = new CloudEvent();
            s_formatter.DecodeBinaryModeEventData(memory, cloudEvent);

            var data = (MessagePublishedData) cloudEvent.Data;
            cloudEvent.Id = data.Message.MessageId;
            cloudEvent.DataContentType = "application/json";
            // TODO: the source should actually be the topic, not the subscription. It's unclear whether that's available though.
            // We could potentially configure it in PubSubFunctionOptions. We should also populate the "topic" CloudEvent
            // attribute to replicate Eventarc behavior.
            cloudEvent.Source = new Uri($"//pubsub.googleapis.com/{data.Subscription}", UriKind.RelativeOrAbsolute);
            cloudEvent.Type = MessagePublishedData.MessagePublishedCloudEventType;
            cloudEvent.Time = DateTimeOffset.UtcNow;

            _logger.LogInformation("Proxying message ID {id} to {url}", data.Message.MessageId, _options.Url);

            var client = _httpClientFactory.CreateClient();
            // Now send a fully-valid CloudEvent request to the real function.
            await client.PostAsync(_options.Url, cloudEvent.ToHttpContent(ContentMode.Binary, s_formatter));
        }
    }
}
