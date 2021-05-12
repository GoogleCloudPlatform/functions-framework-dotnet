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
using System.Threading.Tasks;

namespace Google.Cloud.Functions.Framework
{
    /// <summary>
    /// An adapter to implement an HTTP Function based on a CloudEvent Function.
    /// </summary>
    public sealed class CloudEventAdapter : IHttpFunction
    {
        private readonly ICloudEventFunction _function;
        private readonly ILogger _logger;

        /// <summary>
        /// Constructs a new instance based on the given CloudEvent Function.
        /// </summary>
        /// <param name="function">The CloudEvent Function to invoke.</param>
        /// <param name="logger">The logger to use to report errors.</param>
        public CloudEventAdapter(ICloudEventFunction function, ILogger<CloudEventAdapter> logger)
        {
            _function = Preconditions.CheckNotNull(function, nameof(function));
            _logger = Preconditions.CheckNotNull(logger, nameof(logger));
        }

        /// <summary>
        /// Handles an HTTP request by extracting the CloudEvent from it and passing it to the
        /// original CloudEvent Function. The request fails if it does not contain a CloudEvent.
        /// </summary>
        /// <param name="context">The HTTP context containing the request and response.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task HandleAsync(HttpContext context)
        {
            CloudEvent cloudEvent;

            try
            {
                cloudEvent = await CloudEventConverter.ConvertRequest(context.Request);
            }
            catch (CloudEventConverter.ConversionException e)
            {
                context.Response.StatusCode = 400;
                _logger.LogError(e.Message);
                return;
            }
            await _function.HandleAsync(cloudEvent, context.RequestAborted);
        }
    }
}
