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
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Google.Cloud.Functions.Framework
{
    /// <summary>
    /// An adapter to implement an HTTP function based on an <see cref="ICloudEventFunctionWithReply"/>,
    /// automatically publishing any reply event via an <see cref="ICloudEventPublisher"/>.
    /// </summary>
    public class CloudEventWithReplyAdapter : IHttpFunction
    {
        private readonly ICloudEventFunctionWithReply _function;
        private readonly ICloudEventPublisher _publisher;
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of the adapter.
        /// </summary>
        /// <param name="function">The function to handle the event.</param>
        /// <param name="publisher">The event publisher to use if the function returns a new event.</param>
        /// <param name="logger">The logger to use in case of failures when deserializing.</param>
        public CloudEventWithReplyAdapter(
            ICloudEventFunctionWithReply function,
            ICloudEventPublisher publisher,
            ILogger<CloudEventWithReplyAdapter> logger) =>
            (_function, _publisher, _logger) = (function, publisher, logger);

        /// <inheritdoc />
        public async Task HandleAsync(HttpContext context)
        {
            var cloudEvent = await context.Request.ReadCloudEventAsync();
            // Note: ReadCloudEventAsync appears never to actually return null as it's documented to.
            // Instead, we use the presence of properties required by the spec to determine validity.
            if (!IsValidEvent(cloudEvent))
            {
                context.Response.StatusCode = 400;
                _logger.LogError("Request did not contain a valid cloud event");
                return;
            }
            var reply = await _function.HandleAsync(cloudEvent, context.RequestAborted);
            if (reply is object)
            {
                await _publisher.PublishAsync(reply, context.RequestAborted);
            }
        }

        private static bool IsValidEvent([NotNullWhen(true)] CloudEvent? cloudEvent) =>
            cloudEvent is object &&
            !string.IsNullOrEmpty(cloudEvent.Id) &&
            cloudEvent.Source is object &&
            !string.IsNullOrEmpty(cloudEvent.Type);
    }
}
