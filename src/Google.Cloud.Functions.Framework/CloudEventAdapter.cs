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
using CloudNative.CloudEvents.AspNetCore;
using CloudNative.CloudEvents.Http;
using Google.Cloud.Functions.Framework.GcfEvents;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Google.Cloud.Functions.Framework
{
    /// <summary>
    /// An adapter to implement an HTTP Function based on a CloudEvent Function.
    /// </summary>
    public sealed class CloudEventAdapter : IHttpFunction
    {
        private readonly ICloudEventFunction _function;
        private readonly CloudEventFormatter _formatter;
        private readonly ILogger _logger;

        /// <summary>
        /// Constructs a new instance based on the given CloudEvent Function.
        /// </summary>
        /// <param name="function">The CloudEvent Function to invoke.</param>
        /// <param name="formatter">The CloudEvent formatter to use when deserializing requests.</param>
        /// <param name="logger">The logger to use to report errors.</param>
        public CloudEventAdapter(ICloudEventFunction function, CloudEventFormatter formatter, ILogger<CloudEventAdapter> logger)
        {
            _function = Preconditions.CheckNotNull(function, nameof(function));
            _formatter = Preconditions.CheckNotNull(formatter, nameof(formatter));
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
                cloudEvent = await ConvertRequestAsync(context.Request, _formatter);
            }
            catch (Exception e)
            {
                context.Response.StatusCode = 400;
                _logger.LogError(e.Message);
                return;
            }
            await _function.HandleAsync(cloudEvent, context.RequestAborted);
        }

        /// <summary>
        /// Converts an HTTP request into a CloudEvent, either using regular CloudEvent parsing,
        /// or GCF event conversion if necessary.
        /// </summary>
        /// <param name="request">The request to convert.</param>
        /// <param name="formatter">The formatter to use for conversion.</param>
        /// <returns>A valid CloudEvent</returns>
        internal static Task<CloudEvent> ConvertRequestAsync(HttpRequest request, CloudEventFormatter formatter) =>
            request.IsCloudEvent()
            ? request.ToCloudEventAsync(formatter)
            : GcfConverters.ConvertGcfEventToCloudEvent(request, formatter);
    }
}
