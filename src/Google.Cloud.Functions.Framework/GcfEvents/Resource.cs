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

using System.Text.Json.Serialization;

namespace Google.Cloud.Functions.Framework.GcfEvents
{
    /// <summary>
    /// The resource that an event applies to.
    /// </summary>
    public sealed class Resource
    {
        /// <summary>
        /// The service that triggered the event.
        /// </summary>
        [JsonPropertyName("service")]
        public string? Service { get; set; }

        /// <summary>
        /// The name associated with the event.
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// The type of the resource.
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        // TODO: Do we want raw path? It looks like it's deprecated.
    }
}
