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

#nullable enable

using System.Collections.Generic;
using System.Text.Json.Serialization;

// Note: we'd expect to generate this type from a JSON schema.

namespace Google.Events.SystemTextJson.Cloud.PubSub.V1
{
    /// <summary>
    /// The CloudEvent representation of a PubSub message.
    /// </summary>
    public sealed partial class PubsubMessageData
    {
        /// <summary>
        /// The message type, e.g. "type.googleapis.com/google.pubsub.v1.PubsubMessage".
        /// </summary>
        [JsonPropertyName("@type")]
        public string? Type { get; set; }

        /// <summary>
        /// Attributes for the message. If this is not provided in the event data directly,
        /// it will be an empty dictionary.
        /// </summary>
        [JsonPropertyName("attributes")]
        public IDictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// The binary data in the event.
        /// </summary>
        [JsonPropertyName("data")]
        public byte[]? Data { get; set; }
    }
}
