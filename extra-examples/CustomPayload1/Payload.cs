// Copyright 2021, Google LLC
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
using CloudNative.CloudEvents.SystemTextJson;
using System.Text.Json.Serialization;

namespace CustomPayload1
{
    [CloudEventFormatter(typeof(JsonEventFormatter<Payload>))]
    public class Payload
    {
        // Note: System.Text.Json is case-sensitive. The Newtonsoft.Json
        // deserializer wouldn't require the attributes. (Or if the JSON
        // matched the property name casing, we wouldn't need them then either.)
        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("number")]
        public double Number { get; set; }
    }
}
