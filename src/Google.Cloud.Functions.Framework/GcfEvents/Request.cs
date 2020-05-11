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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text.Json;

namespace Google.Cloud.Functions.Framework.GcfEvents
{
    // TODO: Use System.Text.Json instead of Json.NET here.
    // We already have an implicit Json.NET dependency due to the CloudEvents SDK, but it would be
    // nice not to use it. But JObject is really convenient...
    internal sealed class Request
    {
        [JsonProperty("context")]
        public Context Context { get; set; } = new Context();

        [JsonProperty("data")]
        public JObject Data { get; set; }

        [JsonProperty("resource")]
        public string? Resource { get; set; }

        [JsonProperty("timestamp")]
        public DateTimeOffset? Timestamp { get; set; }

        [JsonProperty("eventType")]
        public string? EventType { get; set; }

        [JsonProperty("eventId")]
        public string? EventId { get; set; }

        [JsonProperty("params")]
        public JObject? Params { get; set; }

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
