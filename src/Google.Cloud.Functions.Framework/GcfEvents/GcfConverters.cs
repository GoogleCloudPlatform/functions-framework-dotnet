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
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;

namespace Google.Cloud.Functions.Framework.GcfEvents
{
    /// <summary>
    /// Conversions from GCF events to CloudEvents.
    /// </summary>
    internal static class GcfConverters
    {
        private static readonly string JsonContentTypeText = "application/json";
        private static readonly ContentType JsonContentType = new ContentType(JsonContentTypeText);

        private static readonly Dictionary<string, string> s_eventTypeMapping = new Dictionary<string, string>
        {
            { "google.pubsub.topic.publish", "com.google.cloud.pubsub.topic.publish.v0" },
            { "google.storage.object.finalize", "com.google.cloud.storage.object.finalize.v0" },
            { "google.storage.object.delete", "com.google.cloud.storage.object.delete.v0" },
            { "google.storage.object.archive", "com.google.cloud.storage.object.archive.v0" },
            { "google.storage.object.metadataUpdate", "com.google.cloud.storage.object.metadataUpdate.v0" },
            { "providers/cloud.firestore/eventTypes/document.write", "com.google.cloud.firestore.document.write.v0" },
            { "providers/cloud.firestore/eventTypes/document.create", "com.google.cloud.firestore.document.create.v0" },
            { "providers/cloud.firestore/eventTypes/document.update", "com.google.cloud.firestore.document.update.v0" },
            { "providers/cloud.firestore/eventTypes/document.delete", "com.google.cloud.firestore.document.delete.v0" },
            { "providers/firebase.auth/eventTypes/user.create", "com.google.cloud.firebase.auth.user.create.v0" },
            { "providers/firebase.auth/eventTypes/user.delete", "com.google.cloud.firebase.auth.user.delete.v0" },
            { "providers/google.firebase.analytics/eventTypes/event.log", "com.google.cloud.firebase.analytics.log.v0" },
            { "providers/google.firebase.database/eventTypes/ref.create", "com.google.cloud.firebase.database.create.v0" },
            { "providers/google.firebase.database/eventTypes/ref.write", "com.google.cloud.firebase.database.write.v0" },
            { "providers/google.firebase.database/eventTypes/ref.update", "com.google.cloud.firebase.database.update.v0" },
            { "providers/google.firebase.database/eventTypes/ref.delete", "com.google.cloud.firebase.database.delete.v0" },
            { "providers/cloud.pubsub/eventTypes/topic.publish", "com.google.cloud.pubsub.topic.publish.v0" },
            { "providers/cloud.storage/eventTypes/object.change", "com.google.cloud.storage.object.change.v0" },
        };

        /// <summary>
        /// Mappings from GCF event types to services, for event types which don't include the service in the context.
        /// </summary>
        private static readonly Dictionary<string, string> s_serviceMapping = new Dictionary<string, string>
        {
            { "providers/cloud.firestore/eventTypes/document.write", "firestore.googleapis.com" },
            { "providers/cloud.firestore/eventTypes/document.create", "firestore.googleapis.com" },
            { "providers/cloud.firestore/eventTypes/document.update", "firestore.googleapis.com" },
            { "providers/cloud.firestore/eventTypes/document.delete", "firestore.googleapis.com" },
            { "providers/google.firebase.analytics/eventTypes/event.log", "firebase.googleapis.com" },
            { "providers/firebase.auth/eventTypes/user.create", "firebase.googleapis.com" },
            { "providers/firebase.auth/eventTypes/user.delete", "firebase.googleapis.com" },
            { "providers/google.firebase.database/eventTypes/ref.create", "firebase.googleapis.com" },
            { "providers/google.firebase.database/eventTypes/ref.write", "firebase.googleapis.com" },
            { "providers/google.firebase.database/eventTypes/ref.update", "firebase.googleapis.com" },
            { "providers/google.firebase.database/eventTypes/ref.delete", "firebase.googleapis.com" },
            { "providers/cloud.pubsub/eventTypes/topic.publish", "pubsub.googleapis.com" },
            { "providers/cloud.storage/eventTypes/object.change", "storage.googleapis.com" },
        };

        internal static async ValueTask<CloudEvent> ConvertGcfEventToCloudEvent(HttpRequest request)
        {
            var jsonRequest = await ParseRequest(request);
            // Validated as not null or empty in ParseRequest
            string gcfType = jsonRequest.Context.Type!;
            if (!s_eventTypeMapping.TryGetValue(gcfType, out var cloudEventType))
            {
                throw new CloudEventConverter.ConversionException($"Unexpected event type for function: '{gcfType}'");
            }

            // Firestore and Firebase database GCF requests include a "params" element that maps wilcards in the resource
            // template to their value in the changed document.
            if (gcfType.StartsWith("providers/cloud.firestore/", StringComparison.Ordinal) ||
                gcfType.StartsWith("providers/google.firebase.database/", StringComparison.Ordinal))
            {
                if (jsonRequest.Params is object)
                {
                    jsonRequest.Data["wildcards"] = jsonRequest.Params;
                }
            }

            var context = jsonRequest.Context;
            var resource = context.Resource;
            var service = ValidateNotNullOrEmpty(resource.Service ?? s_serviceMapping.GetValueOrDefault(gcfType), "service");
            var name = ValidateNotNullOrEmpty(resource.Name, "resource name");
            var source = new Uri($"//{service}/{name}");
            return new CloudEvent(cloudEventType, source, context.Id, context.Timestamp?.UtcDateTime)
            {
                Data = JsonSerializer.Serialize(jsonRequest.Data),
                DataContentType = JsonContentType
            };
        }

        private static async ValueTask<Request> ParseRequest(HttpRequest request)
        {
            Request parsedRequest;
            if (request.ContentType != JsonContentTypeText)
            {
                throw new CloudEventConverter.ConversionException("Unable to convert request to CloudEvent.");
            }
            try
            {
                var json = await new StreamReader(request.Body).ReadToEndAsync();
                parsedRequest = JsonSerializer.Deserialize<Request>(json);
            }
            catch (JsonException e)
            {
                // Note: the details of the original exception will be lost in the normal case (where it's just logged)
                // but keeping in the ConversionException is useful for debugging purposes.
                throw new CloudEventConverter.ConversionException($"Error parsing GCF event: {e.Message}", e);
            }
            
            parsedRequest.NormalizeContext();
            if (parsedRequest.Data is null ||
                string.IsNullOrEmpty(parsedRequest.Context.Id) ||
                string.IsNullOrEmpty(parsedRequest.Context.Type))
            {
                throw new CloudEventConverter.ConversionException("Event is malformed; does not contain a payload, or the event ID is missing.");
            }
            return parsedRequest;
        }

        private static string ValidateNotNullOrEmpty(string? value, string name) =>
            string.IsNullOrEmpty(value)
            ? throw new CloudEventConverter.ConversionException($"Event contained no {name}")
            : value;
    }
}
