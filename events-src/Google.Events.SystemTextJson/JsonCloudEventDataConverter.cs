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

using CloudNative.CloudEvents;
using System;
using System.Net.Mime;
using System.Text.Json;

namespace Google.Events.SystemTextJson
{
    /// <summary>
    /// A CloudEvent data converter using System.Text.Json to parse the JSON.
    /// </summary>
    /// <typeparam name="T">The data type to convert to and from.</typeparam>
    public class JsonCloudEventDataConverter<T> : ICloudEventDataConverter<T> where T : class
    {
        private static readonly JsonSerializerOptions s_jsonSerializerOptions = new JsonSerializerOptions { IgnoreNullValues = true };
        private static readonly ContentType s_jsonContentType = new ContentType("application/json");

        /// <summary>
        /// Constructs an event data converter for the type <typeparamref name="T"/>.
        /// </summary>
        public JsonCloudEventDataConverter()
        {
        }

        // TODO: Check the DataContentType.
        /// <inheritdoc />
        public T ConvertEventData(CloudEvent cloudEvent) =>
            cloudEvent.Data switch
            {
                string json => JsonSerializer.Deserialize<T>(json),
                null => throw new ArgumentException($"CloudEvent contained no data"),
                var data => throw new ArgumentException($"CloudEvent data type was {data.GetType()}")
            };

        // TODO: Check the DataContentType.
        /// <inheritdoc />
        public void PopulateCloudEvent(CloudEvent cloudEvent, T data)
        {
            // TODO: Check that data is of the right type? Look at DataContentType to
            // see if the user wants protobuf binary?
            cloudEvent.Data = JsonSerializer.Serialize(data, s_jsonSerializerOptions);
            cloudEvent.DataContentType = s_jsonContentType;
        }
    }
}
