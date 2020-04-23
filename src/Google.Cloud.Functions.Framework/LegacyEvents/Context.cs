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
using System.Text.Json.Serialization;

namespace Google.Cloud.Functions.Framework.LegacyEvents
{
    /// <summary>
    /// The context of an event.
    /// </summary>
    public sealed class Context
    {
        /// <summary>
        /// A unique ID for the event.
        /// </summary>
        [JsonPropertyName("eventId")]
        public string? Id { get; set; }

        /// <summary>
        /// The date/time this event was created.
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTimeOffset? Timestamp { get; set; }

        /// <summary>
        /// The type of the event. For example: "google.pubsub.topic.publish".
        /// </summary>
        [JsonPropertyName("eventType")]
        public string? Type { get; set; }

        /// <summary>
        /// The resource associated with the event.
        /// </summary>
        [JsonPropertyName("resource")]
        public Resource Resource { get; set; } = new Resource();
    }
}
