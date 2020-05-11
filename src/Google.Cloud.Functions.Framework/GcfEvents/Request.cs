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

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Google.Cloud.Functions.Framework.GcfEvents
{
    internal sealed class Request
    {
        [JsonPropertyName("context")]
        public Context Context { get; set; } = new Context();

        [JsonPropertyName("data")]
        public Dictionary<string, object> Data { get; set; }

        [JsonPropertyName("resource")]
        public string? Resource { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTimeOffset? Timestamp { get; set; }

        [JsonPropertyName("eventType")]
        public string? EventType { get; set; }

        [JsonPropertyName("eventId")]
        public string? EventId { get; set; }

        [JsonPropertyName("params")]
        public dynamic? Params { get; set; }

        public Request()
        {
            Context = null!;
            Data = null!;
        }

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
