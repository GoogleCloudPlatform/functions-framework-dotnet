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

using System.Text;
using System.Text.Json.Serialization;

// This file contains manual additions to the code generated from the JSON schema.

namespace Google.Events.SystemTextJson.Cloud.PubSub.V1
{
    [CloudEventDataConverter(typeof(JsonCloudEventDataConverter<PubsubMessageData>))]
    public partial class PubsubMessageData
    {
        // TODO: Work out whether this should actually be a GetTextData()/SetTextData() method pair,
        // based on the performance characteristics.

        /// <summary>
        /// A UTF-8 view over <see cref="Data"/>. Note that this will decode/encode the data on
        /// each property get/set call, so it's best to store the result in a local variable instead
        /// of calling the property multiple times.
        /// </summary>
        [JsonIgnore]
        public string? TextData
        {
            get => Data is null ? null : Encoding.UTF8.GetString(Data);
            set => Data = value is null ? null : Encoding.UTF8.GetBytes(value);
        }
    }
}
