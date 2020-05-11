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
using Google.Cloud.Functions.Framework.GcfEvents;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Google.Cloud.Functions.Framework
{
    internal static class CloudEventConverter
    {
        /// <summary>
        /// Converts an HTTP request into a CloudEvent, either using regular CloudEvent parsing,
        /// or GCF event conversion if necessary.
        /// </summary>
        /// <param name="request">The request to convert.</param>
        /// <exception cref="ConversionException">The Cloud Event couldn't be converted.</exception>
        /// <returns>A valid CloudEvent</returns>
        internal static ValueTask<CloudEvent> ConvertRequest(HttpRequest request) =>
            IsNativeCloudEventRequest(request)
            ? ConvertNativeCloudEventRequest(request)
            : GcfConverters.ConvertGcfEventToCloudEvent(request);

        private static bool IsNativeCloudEventRequest(HttpRequest request) =>
            // HTTP CloudEvent requests have three modes - see https://github.com/cloudevents/spec/blob/v1.0/http-protocol-binding.md
            // Batched or structured mode
            request.ContentType?.StartsWith(CloudEvent.MediaType, StringComparison.InvariantCultureIgnoreCase) == true ||
            // Binary mode
            (request.Headers.ContainsKey("ce-type") && request.Headers.ContainsKey("ce-source") && request.Headers.ContainsKey("ce-id"));

        private static async ValueTask<CloudEvent> ConvertNativeCloudEventRequest(HttpRequest request)
        {
            var cloudEvent = await request.ReadCloudEventAsync();
            return IsValidEvent(cloudEvent) ? cloudEvent : throw new ConversionException("Request did not contain a valid Cloud Event");

            // Note: ReadCloudEventAsync appears never to actually return null as it's documented to.
            // Instead, we use the presence of properties required by the spec to determine validity.
            bool IsValidEvent(CloudEvent? cloudEvent) =>
                cloudEvent is object &&
                !string.IsNullOrEmpty(cloudEvent.Id) &&
                cloudEvent.Source is object &&
                !string.IsNullOrEmpty(cloudEvent.Type);
        }

        /// <summary>
        /// Exception thrown to indicate that the conversion of an HTTP request
        /// into a CloudEvent has failed. This is handled within <see cref="CloudEventAdapter{TData}"></see> and <see cref="CloudEventAdapter"/>.
        /// </summary>
        internal sealed class ConversionException : Exception
        {
            internal ConversionException(string message) : base(message)
            {
            }

            internal ConversionException(string message, Exception innerException) : base(message, innerException)
            {
            }
        }
    }
}
