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
using Google.Protobuf;
using System;
using System.Net.Mime;
using System.Reflection;

namespace Google.Events.Protobuf
{
    /// <summary>
    /// Converts CloudEvent data between the raw serialization format (typically JSON) and a generated Protobuf message.
    /// </summary>
    /// <typeparam name="T">The message type to convert.</typeparam>
    public sealed class ProtobufCloudEventDataConverter<T> : ICloudEventDataConverter<T> where T : class, IMessage<T>, new()
    {
        private static readonly ContentType s_jsonContentType = new ContentType("application/json");
        // TODO: Check this is the one we want to use.
        private static readonly ContentType s_protobufContentType = new ContentType("application/protobuf");
        private static readonly JsonParser s_jsonParser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));
        
        /// <summary>
        /// Constructs an event data converter for the Protobuf message <typeparamref name="T"/>.
        /// </summary>
        public ProtobufCloudEventDataConverter()
        {
        }

        /// <inheritdoc />
        public T ConvertEventData(CloudEvent cloudEvent)
        {
            ProtoPreconditions.CheckNotNull(cloudEvent, nameof(cloudEvent));

            // TODO: Check the DataContentType matches the data?
            return cloudEvent.Data switch
            {
                byte[] bytes => ParseBinary(bytes),
                string json => s_jsonParser.Parse<T>(json),
                null => throw new ArgumentException($"CloudEvent contained no data"),
                var data => throw new ArgumentException($"CloudEvent data type was {data.GetType()}")
            };

            // Annoyingly, we can't inline this in the switch expression because MergeFrom doesn't return T.
            T ParseBinary(byte[] bytes)
            {
                var message = new T();
                message.MergeFrom(bytes);
                return message;
            }
        }

        /// <inheritdoc />
        public void PopulateCloudEvent(CloudEvent cloudEvent, T data)
        {
            ProtoPreconditions.CheckNotNull(cloudEvent, nameof(cloudEvent));
            ProtoPreconditions.CheckNotNull(data, nameof(data));
            var contentType = cloudEvent.DataContentType;
            if (contentType is null)
            {
                cloudEvent.Data = JsonFormatter.Default.Format(data);
                cloudEvent.DataContentType = s_jsonContentType;
            }
            else if (contentType.Equals(s_jsonContentType))
            {
                cloudEvent.Data = JsonFormatter.Default.Format(data);
            }
            else if (contentType.Equals(s_protobufContentType))
            {
                cloudEvent.Data = data.ToByteArray();
            }
            else
            {
                throw new ArgumentException($"Unable to serialize to requested content type '{contentType}'");
            }
        }
    }
}
