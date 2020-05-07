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
using System;

namespace Google.Cloud.Functions.Framework.GcfEvents
{
    /// <summary>
    /// The context of a GCF event.
    /// </summary>
    internal sealed class Context
    {
        // Note: maps to CloudEvents ID
        /// <summary>
        /// A unique ID for the event.
        /// </summary>
        [JsonProperty("eventId")]
        public string? Id { get; set; }

        // Note: maps to CloudEvents time (optional)
        /// <summary>
        /// The date/time this event was created.
        /// </summary>
        [JsonProperty("timestamp")]
        public DateTimeOffset? Timestamp { get; set; }

        // Note: maps to CloudEvents type (possibly via additional mapping)
        /// <summary>
        /// The type of the event. For example: "google.pubsub.topic.publish".
        /// </summary>
        [JsonProperty("eventType")]
        public string? Type { get; set; }

        // Maps somewhat to the CloudEvents subject (optional) and subject (required)
        /// <summary>
        /// The resource associated with the event.
        /// </summary>
        [JsonProperty("resource")]
        public Resource Resource { get; set; } = new Resource();
    }
}
