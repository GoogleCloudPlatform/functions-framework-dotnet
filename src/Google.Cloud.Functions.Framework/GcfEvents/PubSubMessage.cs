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

using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Google.Cloud.Functions.Framework.GcfEvents
{
    /// <summary>
    /// The CloudEvent representation of a PubSub message as translated from a GCF event.
    /// </summary>
    public sealed class PubSubMessage
    {
        internal const string PublishEventType = "com.google.cloud.pubsub.topic.publish.v0";

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

        // TODO: Work out whether this should actually be a GetTextData()/SetTextData() method pair,
        // based on the performance characteristics.

        /// <summary>
        /// A UTF-8 view over <see cref="Data"/>. Note that this will decode/encode the data on
        /// each property get/set call, so it's best to store the result in a local variable instead
        /// of calling the property multiple times.
        /// </summary>
        public string? TextData
        {
            get => Data is null ? null : Encoding.UTF8.GetString(Data);
            set => Data = value is null ? null : Encoding.UTF8.GetBytes(value);
        }
    }
}
