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
using CloudNative.CloudEvents.Core;
using CloudNative.CloudEvents.SystemTextJson;
using Google.Events.Protobuf.Cloud.Firestore.V1;
using Google.Events.Protobuf.Cloud.PubSub.V1;
using Google.Events.Protobuf.Cloud.Storage.V1;
using Google.Events.Protobuf.Firebase.Database.V1;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace GoogleTypeDetection
{
    /// <summary>
    /// A CloudEvent formatter that knows about all the events in the Google.Events.Protobuf package.
    /// This would be in that package.
    /// </summary>
    public class GoogleEventFormatter : CloudEventFormatter
    {
        private static Dictionary<string, MessageDescriptor> s_eventTypeToPayloadType = new Dictionary<string, MessageDescriptor>
        {
            { StorageObjectData.ArchivedCloudEventType, StorageObjectData.Descriptor },
            { StorageObjectData.DeletedCloudEventType, StorageObjectData.Descriptor },
            { StorageObjectData.FinalizedCloudEventType, StorageObjectData.Descriptor },
            { StorageObjectData.MetadataUpdatedCloudEventType, StorageObjectData.Descriptor },

            { MessagePublishedData.MessagePublishedCloudEventType, MessagePublishedData.Descriptor },

            { DocumentEventData.CreatedCloudEventType, DocumentEventData.Descriptor },
            { DocumentEventData.DeletedCloudEventType, DocumentEventData.Descriptor },

            { ReferenceEventData.CreatedCloudEventType, ReferenceEventData.Descriptor },
            { ReferenceEventData.DeletedCloudEventType, ReferenceEventData.Descriptor },
            { ReferenceEventData.WrittenCloudEventType, ReferenceEventData.Descriptor },
            { ReferenceEventData.UpdatedCloudEventType, ReferenceEventData.Descriptor },

            // Etc... this would all be generated.
        };

        private static readonly JsonParser s_jsonParser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));

        // Note: although we delegate to a JsonEventFormatter (via StructuredEventFormatter) and *could* just derive from JsonEventFormatter,
        // that would provide less flexibility for the future.
        private static readonly CloudEventFormatter s_structuredEventFormatter = new StructuredEventFormatter();

        /// <inheritdoc />
        public override void DecodeBinaryModeEventData(ReadOnlyMemory<byte> body, CloudEvent cloudEvent)
        {
            if (cloudEvent.Type is string eventType && s_eventTypeToPayloadType.TryGetValue(eventType, out var descriptor))
            {
                cloudEvent.Data = body.Length == 0
                    ? null
                    : s_jsonParser.Parse(new StreamReader(BinaryDataUtilities.AsStream(body)), descriptor);
                return;
            }
            throw new ArgumentException("Unknown CloudEvent type: " + cloudEvent.Type);
        }

        /// <inheritdoc />
        public override CloudEvent DecodeStructuredModeMessage(ReadOnlyMemory<byte> body, ContentType contentType, IEnumerable<CloudEventAttribute> extensionAttributes) =>
            s_structuredEventFormatter.DecodeStructuredModeMessage(body, contentType, extensionAttributes);

        /// <inheritdoc />
        public override IReadOnlyList<CloudEvent> DecodeBatchModeMessage(ReadOnlyMemory<byte> body, ContentType contentType, IEnumerable<CloudEventAttribute> extensionAttributes) =>
            s_structuredEventFormatter.DecodeBatchModeMessage(body, contentType, extensionAttributes);

        /// <inheritdoc />
        public override ReadOnlyMemory<byte> EncodeBinaryModeEventData(CloudEvent cloudEvent)
        {
            var data = (IMessage) cloudEvent.Data;
            if (data is null)
            {
                return Array.Empty<byte>();
            }
            var json = data.ToString();
            return Encoding.UTF8.GetBytes(json);
        }

        /// <inheritdoc />
        public override ReadOnlyMemory<byte> EncodeStructuredModeMessage(CloudEvent cloudEvent, out ContentType contentType) =>
            s_structuredEventFormatter.EncodeStructuredModeMessage(cloudEvent, out contentType);

        /// <inheritdoc />
        public override ReadOnlyMemory<byte> EncodeBatchModeMessage(IEnumerable<CloudEvent> cloudEvents, out ContentType contentType) =>
            s_structuredEventFormatter.EncodeBatchModeMessage(cloudEvents, out contentType);

        // This is basically a slightly-modified copy of the code within ProtobufJsonCloudEventFormatter<T>.
        private class StructuredEventFormatter : JsonEventFormatter
        {
            protected override void EncodeStructuredModeData(CloudEvent cloudEvent, Utf8JsonWriter writer)
            {
                writer.WritePropertyName("data");
                // TODO: Try to make this cleaner. It's annoying that we have to convert to a JSON string,
                // reparse, then reserialize.
                using var document = JsonDocument.Parse(cloudEvent.Data!.ToString());
                document.WriteTo(writer);
            }

            protected override void DecodeStructuredModeDataProperty(JsonElement dataElement, CloudEvent cloudEvent)
            {
                if (dataElement.ValueKind != JsonValueKind.Object)
                {
                    throw new InvalidOperationException("Expected Data property to have an object value");
                }
                if (cloudEvent.Type is string eventType && s_eventTypeToPayloadType.TryGetValue(eventType, out var descriptor))
                {
                    string json = dataElement.GetRawText();
                    cloudEvent.Data = s_jsonParser.Parse(json, descriptor);
                    return;
                }
            }

            protected override void DecodeStructuredModeDataBase64Property(JsonElement dataBase64Element, CloudEvent cloudEvent)
            {
                throw new InvalidOperationException("Expected Data property to be set");
            }
        }
    }
}
