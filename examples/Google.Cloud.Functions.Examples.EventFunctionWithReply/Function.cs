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
using Microsoft.Extensions.Logging;
using System;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

namespace Google.Cloud.Functions.Examples.EventFunctionWithReply
{
    // Just a demo function that publishes a reply with a sample event.
    public class Function : ICloudEventFunctionWithReply
    {
        private readonly ILogger _logger;

        public Function(ILogger<Function> logger) => _logger = logger;

        /// <inheritdoc />
        public Task<CloudEvent?> HandleAsync(CloudEvent cloudEvent, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling event {id}", cloudEvent.Id);

            var newEvent = new CloudEvent(
                type: "reply-type",
                source: new Uri("http://127.0.0.1/event/source"),
                id: Guid.NewGuid().ToString(),
                time: DateTime.UtcNow)
            {
                Data = $"This is a reply to {cloudEvent.Id}",
                DataContentType = new ContentType("text/plain")
            };

            _logger.LogInformation("Returning event {id}", newEvent.Id);
            return Task.FromResult<CloudEvent?>(newEvent);
        }
    }
}
