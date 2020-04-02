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

using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using System.Diagnostics.CodeAnalysis;

namespace Google.Cloud.Functions.Framework
{
    /// <summary>
    /// An adapter to implement an HTTP Function based on a Cloud Event Function.
    /// </summary>
    public sealed class CloudEventAdapter : IHttpFunction
    {
        private readonly ICloudEventFunction _function;

        /// <summary>
        /// Constructs a new instance based on the given Cloud Event Function.
        /// </summary>
        /// <param name="function">The Cloud Event Function to invoke.</param>
        public CloudEventAdapter(ICloudEventFunction function) =>
            _function = Preconditions.CheckNotNull(function, nameof(function));

        /// <summary>
        /// Handles an HTTP request by extracting the Cloud Event from it and passing it to the
        /// original Cloud Event Function. The request fails if it does not contain a Cloud Event.
        /// </summary>
        /// <param name="context">The HTTP context containing the request and response.</param>
        /// <returns></returns>
        public async Task HandleAsync(HttpContext context)
        {
            var cloudEvent = await context.Request.ReadCloudEventAsync();
            // Note: ReadCloudEventAsync appears never to actually return null as it's documented to.
            // Instead, we use the presence of properties required by the spec to determine validity.
            if (!IsValidEvent(cloudEvent))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Request is expected to contain a Cloud Event.", context.RequestAborted);
                return;
            }
            await _function.HandleAsync(cloudEvent);
        }

        private static bool IsValidEvent([NotNullWhen(true)] CloudEvent? cloudEvent) =>
            cloudEvent is object &&
            !string.IsNullOrEmpty(cloudEvent.Id) &&
            cloudEvent.Source is object &&
            !string.IsNullOrEmpty(cloudEvent.Type);
    }
}
