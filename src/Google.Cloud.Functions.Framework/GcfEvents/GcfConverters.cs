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
using System.Text.RegularExpressions;
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

        private static readonly Dictionary<string, EventAdapter> s_eventTypeMapping = new Dictionary<string, EventAdapter>
        {
            { "google.pubsub.topic.publish", new PubSubEventAdapter("com.google.cloud.pubsub.topic.publish.v0") },
            { "google.storage.object.finalize", new StorageEventAdapter("com.google.cloud.storage.object.finalize.v0") },
            { "google.storage.object.delete", new StorageEventAdapter("com.google.cloud.storage.object.delete.v0") },
            { "google.storage.object.archive", new StorageEventAdapter("com.google.cloud.storage.object.archive.v0") },
            { "google.storage.object.metadataUpdate", new StorageEventAdapter("com.google.cloud.storage.object.metadataUpdate.v0") },
            { "providers/cloud.firestore/eventTypes/document.write",
                new FirestoreFirebaseEventAdapter("com.google.cloud.firestore.document.write.v0", "firestore.googleapis.com") },
            { "providers/cloud.firestore/eventTypes/document.create",
                new FirestoreFirebaseEventAdapter("com.google.cloud.firestore.document.create.v0", "firestore.googleapis.com") },
            { "providers/cloud.firestore/eventTypes/document.update",
                new FirestoreFirebaseEventAdapter("com.google.cloud.firestore.document.update.v0", "firestore.googleapis.com") },
            { "providers/cloud.firestore/eventTypes/document.delete",
                new FirestoreFirebaseEventAdapter("com.google.cloud.firestore.document.delete.v0", "firestore.googleapis.com") },
            { "providers/firebase.auth/eventTypes/user.create",
                new FirestoreFirebaseEventAdapter("com.google.cloud.firebase.auth.user.create.v0", "firebase.googleapis.com") },
            { "providers/firebase.auth/eventTypes/user.delete",
                new FirestoreFirebaseEventAdapter("com.google.cloud.firebase.auth.user.delete.v0", "firebase.googleapis.com") },
            { "providers/google.firebase.analytics/eventTypes/event.log",
                new FirestoreFirebaseEventAdapter("com.google.cloud.firebase.analytics.log.v0", "firebase.googleapis.com") },
            { "providers/google.firebase.database/eventTypes/ref.create",
                new FirestoreFirebaseEventAdapter("com.google.cloud.firebase.database.create.v0", "firebase.googleapis.com") },
            { "providers/google.firebase.database/eventTypes/ref.write",
                new FirestoreFirebaseEventAdapter("com.google.cloud.firebase.database.write.v0", "firebase.googleapis.com") },
            { "providers/google.firebase.database/eventTypes/ref.update",
                new FirestoreFirebaseEventAdapter("com.google.cloud.firebase.database.update.v0", "firebase.googleapis.com") },
            { "providers/google.firebase.database/eventTypes/ref.delete",
                new FirestoreFirebaseEventAdapter("com.google.cloud.firebase.database.delete.v0", "firebase.googleapis.com") },
            { "providers/cloud.pubsub/eventTypes/topic.publish", new PubSubEventAdapter("com.google.cloud.pubsub.topic.publish.v0") },
            { "providers/cloud.storage/eventTypes/object.change", new StorageEventAdapter("com.google.cloud.storage.object.change.v0") },
        };

        internal static async ValueTask<CloudEvent> ConvertGcfEventToCloudEvent(HttpRequest request)
        {
            var jsonRequest = await ParseRequest(request);
            // Validated as not null or empty in ParseRequest
            string gcfType = jsonRequest.Context.Type!;
            if (!s_eventTypeMapping.TryGetValue(gcfType, out var eventAdapter))
            {
                throw new CloudEventConverter.ConversionException($"Unexpected event type for function: '{gcfType}'");
            }
            return eventAdapter.ConvertToCloudEvent(jsonRequest);
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

        /// <summary>
        /// An event-specific adapter, picked based on the event type from the GCF event.
        /// This abstract class provides seams for event-specific changes to the data and the source/subject.
        /// </summary>
        private abstract class EventAdapter
        {
            private readonly string _cloudEventType;
            private readonly string _defaultService;

            protected EventAdapter(string cloudEventType, string defaultService) =>
                (_cloudEventType, _defaultService) = (cloudEventType, defaultService);

            internal CloudEvent ConvertToCloudEvent(Request request)
            {
                MaybeReshapeData(request);

                var context = request.Context;
                var resource = context.Resource;
                var service = ValidateNotNullOrEmpty(resource.Service ?? _defaultService, "service");
                var resourceName = ValidateNotNullOrEmpty(resource.Name, "resource name");
                var (source, subject) = ConvertResourceToSourceAndSubject(service, resourceName);
                var evt = new CloudEvent(_cloudEventType, source, context.Id, context.Timestamp?.UtcDateTime)
                {
                    Data = JsonSerializer.Serialize(request.Data),
                    DataContentType = JsonContentType
                };
                // CloudEvent.Subject is null by default, but doesn't allow null to be set.
                // See https://github.com/cloudevents/sdk-csharp/issues/58
                if (subject is string)
                {
                    evt.Subject = subject;
                }
                return evt;
            }

            protected virtual void MaybeReshapeData(Request request) { }
            protected virtual (Uri source, string? subject) ConvertResourceToSourceAndSubject(string service, string resource) =>
                (new Uri($"//{service}/{resource}"), null);
        }

        private class StorageEventAdapter : EventAdapter
        {
            // Regex to split a storage resource name into bucket and object names.
            // Note the removal of "#" followed by trailing digits at the end of the object name;
            // this represents the object generation and should not be included in the CloudEvent subject.
            private static readonly Regex StorageResourcePattern =
                new Regex(@"^(projects/_/buckets/[^/]+)/(objects/.*?)(?:#\d+)?$", RegexOptions.Compiled);

            internal StorageEventAdapter(string cloudEventType) : base(cloudEventType, "storage.googleapis.com")
            {
            }

            /// <summary>
            /// Split a Storage object resource name into bucket (in the source) and object (in the subject)
            /// </summary>
            protected override (Uri source, string? subject) ConvertResourceToSourceAndSubject(string service, string resource)
            {
                if (StorageResourcePattern.Match(resource) is { Success: true } match)
                {
                    // Resource name up to the bucket
                    resource = match.Groups[1].Value;
                    // Resource name from "objects" onwards, but without the generation
                    var subject = match.Groups[2].Value;
                    return (new Uri($"//{service}/{resource}"), subject);
                }
                // If the resource name didn't match for whatever reason, just use the default behaviour.
                return base.ConvertResourceToSourceAndSubject(service, resource);
            }
        }

        private class PubSubEventAdapter : EventAdapter
        {
            internal PubSubEventAdapter(string cloudEventType) : base(cloudEventType, "pubsub.googleapis.com")
            {
            }

            /// <summary>
            /// Wrap the PubSub message for consistency with Cloud Run.
            /// </summary>
            protected override void MaybeReshapeData(Request request) =>
                request.Data = new Dictionary<string, object> { { "message", request.Data } };
        }

        private class FirestoreFirebaseEventAdapter : EventAdapter
        {
            internal FirestoreFirebaseEventAdapter(string cloudEventType, string defaultService)
                : base(cloudEventType, defaultService)
            {
            }

            /// <summary>
            /// Firestore and Firebase database GCF requests include a "params" element that maps wildcards in the resource
            /// template to their value in the changed document. We map this to a "wildcards" property in the data.
            /// </summary>
            protected override void MaybeReshapeData(Request request)
            {
                if (request.Params is object)
                {
                    request.Data["wildcards"] = request.Params;
                }
            }
        }
    }
}
