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
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Google.Cloud.Functions.Framework.LegacyEvents
{
    /// <summary>
    /// An adapter to implement an HTTP Function based on a Legacy Event Function.
    /// </summary>
    /// <typeparam name="T">The payload type.</typeparam>
    public sealed class LegacyEventAdapter<T> : IHttpFunction where T : class
    {
        private readonly ILegacyEventFunction<T> _function;

        /// <summary>
        /// Constructs a new instance based on the given Legacy Event Function.
        /// </summary>
        /// <param name="function">The Legacy Event Function to invoke.</param>
        public LegacyEventAdapter(ILegacyEventFunction<T> function) =>
            _function = function;

        /// <summary>
        /// Handles an HTTP request by extracting the Cloud Event from it and passing it to the
        /// original Cloud Event Function. The request fails if it does not contain a legacy event
        /// that can be deserialized to the payload type.
        /// </summary>
        /// <param name="context">The HTTP context containing the request and response.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task HandleAsync(HttpContext context)
        {
            Request parsedRequest;
            try
            {
                parsedRequest = await JsonSerializer.DeserializeAsync<Request>(context.Request.Body);
            }
            catch (JsonException)
            {
                context.Response.StatusCode = 400;
                return;
            }
            parsedRequest.NormalizeContext();
            if (parsedRequest.Data is null || parsedRequest.Context?.Id is null)
            {
                context.Response.StatusCode = 400;
                return;
            }

            await _function.HandleAsync(parsedRequest.Data!, parsedRequest.Context!);
        }

        private class Request
        {
            [JsonPropertyName("context")]
            public Context? Context { get; set; }

            [JsonPropertyName("data")]
            public T? Data { get; set; }

            [JsonPropertyName("resource")]
            public string? Resource { get; set; }

            [JsonPropertyName("timestamp")]
            public DateTimeOffset? Timestamp { get; set; }

            [JsonPropertyName("eventType")]
            public string? EventType { get; set; }

            [JsonPropertyName("eventId")]
            public string? EventId { get; set; }

            /// <summary>
            /// Copies any top-level data into the context. Data
            /// already present in the context "wins".
            /// </summary>
            public void NormalizeContext()
            {
                Context ??= new Context();
                Context.Resource ??= new Resource();
                Context.Resource.Name ??= Resource;
                Context.Id ??= EventId;
                Context.Timestamp ??= Timestamp;
                Context.Type ??= EventType;
            }
        }
    }
}
