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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Google.Cloud.Functions.Framework.GcfEvents
{
    /// <summary>
    /// Conversions from GCF events to CloudEvents.
    /// </summary>
    internal static class GcfConverters
    {
        private static readonly ContentType JsonContentType = new ContentType("application/json");
        private static readonly Dictionary<string, string> s_pubSubEventTypeMappings = new Dictionary<string, string>
        {
            { "google.pubsub.topic.publish", "com.google.cloud.pubsub.topic.publish.v0" }
        };

        private static readonly Dictionary<string, string> s_storageEventTypeMappings = new Dictionary<string, string>
        {
            { "google.storage.object.finalize", "com.google.cloud.storage.object.finalize.v0" },
            { "google.storage.object.delete", "com.google.cloud.storage.object.delete.v0" },
            { "google.storage.object.archive", "com.google.cloud.storage.object.archive.v0" },
            { "google.storage.object.metadataUpdate", "com.google.cloud.storage.object.metadataUpdate.v0" },
        };

        private static readonly Dictionary<string, string> s_firestoreEventTypeMappings = new Dictionary<string, string>
        {
            { "providers/cloud.firestore/eventTypes/document.write", "com.google.firestore.document.write.v0" },
            { "providers/cloud.firestore/eventTypes/document.create", "com.google.firestore.document.create.v0" },
            { "providers/cloud.firestore/eventTypes/document.update", "com.google.firestore.document.update.v0" },
            { "providers/cloud.firestore/eventTypes/document.delete", "com.google.firestore.document.delete.v0" },
        };

        internal static async ValueTask<CloudEvent> ConvertPubSubMessage(HttpRequest request)
        {
            var jsonRequest = await ParseRequest(request);
            return CreateEvent(jsonRequest, s_pubSubEventTypeMappings);
        }

        internal static async ValueTask<CloudEvent> ConvertStorageObject(HttpRequest request)
        {
            var jsonRequest = await ParseRequest(request);
            return CreateEvent(jsonRequest, s_storageEventTypeMappings);
        }

        internal static async ValueTask<CloudEvent> ConvertFirestoreEvent(HttpRequest request)
        {
            var jsonRequest = await ParseRequest(request);
            var context = jsonRequest.Context;
            if (jsonRequest.Params is object)
            {
                jsonRequest.Data["wildcards"] = jsonRequest.Params;
            }
            var resourceName = ValidateNotNullOrEmpty(context.Resource.Name, "resource name");

            return CreateEvent(jsonRequest, s_firestoreEventTypeMappings, new Uri($"//firestore.googleapis.com/{resourceName}"));
        }

        private static CloudEvent CreateEvent(Request jsonRequest, Dictionary<string, string> eventTypeMappings, Uri? source = null)
        {
            var context = jsonRequest.Context;
            var resource = context.Resource;
            if (source is null)
            {
                var service = ValidateNotNullOrEmpty(resource.Service, "service");
                var name = ValidateNotNullOrEmpty(resource.Name, "resource name");
                source = new Uri($"//{service}/{name}");
            }
            var type = eventTypeMappings.GetValueOrDefault(context.Type ?? "")
                ?? throw new CloudEventConversionException($"Unexpected event type for function: '{context.Type}'");
            var id = ValidateNotNullOrEmpty(context.Id, "ID");
            return new CloudEvent(type, source, id, context.Timestamp?.UtcDateTime)
            {
                Data = jsonRequest.Data.ToString(Newtonsoft.Json.Formatting.None),
                DataContentType = JsonContentType
            };
        }

        private static async ValueTask<Request> ParseRequest(HttpRequest request)
        {
            Request parsedRequest;
            try
            {
                var json = await new StreamReader(request.Body).ReadToEndAsync();
                parsedRequest = JsonConvert.DeserializeObject<Request>(json);
            }
            catch (JsonException e)
            {
                // TODO: Propagate the original exception?
                throw new CloudEventConversionException($"Error parsing GCF event: {e.Message}");
            }
            
            parsedRequest.NormalizeContext();
            if (parsedRequest.Data is null || parsedRequest.Context?.Id is null)
            {
                throw new CloudEventConversionException("Event is malformed; does not contain a payload, or the event ID is missing.");
            }
            return parsedRequest;
        }

        private static string ValidateNotNullOrEmpty(string? value, string name) =>
            string.IsNullOrEmpty(value)
            ? throw new CloudEventConversionException($"Event contained no {name}")
            : value;
    }
}
