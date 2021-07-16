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
using System.Threading.Tasks;

namespace Google.Cloud.Functions.Framework
{
    /// <summary>
    /// An adapter to implement an HTTP Function based on a CloudEvent Function, with built-in event deserialization.
    /// </summary>
    public sealed class CloudEventAdapter<TData> : IHttpFunction where TData : class
    {
        private readonly ICloudEventFunction<TData> _function;
        private readonly CloudEventFormatter _formatter;
        private readonly ILogger _logger;

        /// <summary>
        /// Constructs a new instance based on the given CloudEvent Function.
        /// </summary>
        /// <param name="function">The CloudEvent Function to invoke.</param>
        /// <param name="formatter">The CloudEvent formatter to use to deserialize the CloudEvent. This is expected to be a single-type formatter,
        /// targeting <typeparamref name="TData"/>.</param>
        /// <param name="logger">The logger to use to report errors.</param>
        public CloudEventAdapter(ICloudEventFunction<TData> function, CloudEventFormatter formatter, ILogger<CloudEventAdapter<TData>> logger)
        {
            _function = Preconditions.CheckNotNull(function, nameof(function));
            if (formatter is null)
            {
                throw new ArgumentNullException(nameof(formatter),
                    "No CloudEventFormatter configured for the CloudEvent function. " + 
                    "The Functions Framework hosting will provide an event formatter if the target " +
                    "data type is decorated with CloudEventFormatterAttribute; otherwise, the formatter " +
                    "should be configured via dependency injection.");
            }
            _formatter = Preconditions.CheckNotNull(formatter, nameof(formatter));
            _logger = Preconditions.CheckNotNull(logger, nameof(logger));
        }

        /// <summary>
        /// Handles an HTTP request by extracting the CloudEvent from it, deserializing the data, and passing
        /// both the event and the data to the original CloudEvent Function.
        /// The request fails if it does not contain a CloudEvent.
        /// </summary>
        /// <param name="context">The HTTP context containing the request and response.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task HandleAsync(HttpContext context)
        {
            CloudEvent cloudEvent;
            TData data;
            try
            {
                cloudEvent = await CloudEventAdapter.ConvertRequestAsync(context.Request, _formatter);
                data = (TData) cloudEvent.Data!;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                context.Response.StatusCode = 400;
                return;
            }

            await _function.HandleAsync(cloudEvent, data, context.RequestAborted);
        }
    }
}
