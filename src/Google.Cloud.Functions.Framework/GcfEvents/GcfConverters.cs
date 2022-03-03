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
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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
        private static class Services
        {
            internal const string FirebaseDatabase = "firebasedatabase.googleapis.com";
            internal const string FirebaseAuth = "firebaseauth.googleapis.com";
            internal const string FirebaseRemoteConfig = "firebaseremoteconfig.googleapis.com";
            internal const string FirebaseAnalytics = "firebaseanalytics.googleapis.com";
            internal const string Firestore = "firestore.googleapis.com";
            internal const string PubSub = "pubsub.googleapis.com";
            internal const string Storage = "storage.googleapis.com";
        }

        private static class EventTypes
        {
            private const string FirestoreDocumentV1 = "google.cloud.firestore.document.v1";
            internal const string FirestoreDocumentCreated = FirestoreDocumentV1 + ".created";
            internal const string FirestoreDocumentUpdated = FirestoreDocumentV1 + ".updated";
            internal const string FirestoreDocumentDeleted = FirestoreDocumentV1 + ".deleted";
            internal const string FirestoreDocumentWritten = FirestoreDocumentV1 + ".written";

            private const string FirebaseDatabaseDocumentV1 = "google.firebase.database.ref.v1";
            internal const string FirebaseDatabaseDocumentCreated = FirebaseDatabaseDocumentV1 + ".created";
            internal const string FirebaseDatabaseDocumentUpdated = FirebaseDatabaseDocumentV1 + ".updated";
            internal const string FirebaseDatabaseDocumentDeleted = FirebaseDatabaseDocumentV1 + ".deleted";
            internal const string FirebaseDatabaseDocumentWritten = FirebaseDatabaseDocumentV1 + ".written";

            private const string FirebaseAuthUserV1 = "google.firebase.auth.user.v1";
            internal const string FirebaseAuthUserCreated = FirebaseAuthUserV1 + ".created";
            internal const string FirebaseAuthUserDeleted = FirebaseAuthUserV1 + ".deleted";

            internal const string FirebaseRemoteConfigUpdated = "google.firebase.remoteconfig.remoteConfig.v1.updated";

            internal const string FirebaseAnalyticsLogWritten = "google.firebase.analytics.log.v1.written";

            internal const string PubSubMessagePublished = "google.cloud.pubsub.topic.v1.messagePublished";

            private const string StorageObjectV1 = "google.cloud.storage.object.v1";
            internal const string StorageObjectFinalized = StorageObjectV1 + ".finalized";
            internal const string StorageObjectArchived = StorageObjectV1 + ".archived";
            internal const string StorageObjectDeleted = StorageObjectV1 + ".deleted";
            internal const string StorageObjectMetadataUpdated = StorageObjectV1 + ".metadataUpdated";

            // Note: this isn't documented in google-cloudevents; it's from the legacy Storage change notification.
            internal const string StorageObjectChanged = StorageObjectV1 + ".changed";
        }

        internal const string DefaultRawPubSubTopic = "projects/unknown-project!/topics/unknown-topic!";
        private const string GcfPubSubTopicPublishType = "google.pubsub.topic.publish";
        private const string JsonContentType = "application/json";

        private static readonly Dictionary<string, EventAdapter> s_eventTypeMapping = new Dictionary<string, EventAdapter>
        {
            { GcfPubSubTopicPublishType, new PubSubEventAdapter(EventTypes.PubSubMessagePublished) },
            { "google.storage.object.finalize", new StorageEventAdapter(EventTypes.StorageObjectFinalized) },
            { "google.storage.object.delete", new StorageEventAdapter(EventTypes.StorageObjectDeleted) },
            { "google.storage.object.archive", new StorageEventAdapter(EventTypes.StorageObjectArchived) },
            { "google.storage.object.metadataUpdate", new StorageEventAdapter(EventTypes.StorageObjectMetadataUpdated) },
            { "providers/cloud.firestore/eventTypes/document.write",
                new FirestoreDocumentEventAdapter(EventTypes.FirestoreDocumentWritten, Services.Firestore) },
            { "providers/cloud.firestore/eventTypes/document.create",
                new FirestoreDocumentEventAdapter(EventTypes.FirestoreDocumentCreated, Services.Firestore) },
            { "providers/cloud.firestore/eventTypes/document.update",
                new FirestoreDocumentEventAdapter(EventTypes.FirestoreDocumentUpdated, Services.Firestore) },
            { "providers/cloud.firestore/eventTypes/document.delete",
                new FirestoreDocumentEventAdapter(EventTypes.FirestoreDocumentDeleted, Services.Firestore) },
            { "providers/firebase.auth/eventTypes/user.create",
                new FirebaseAuthEventAdapter(EventTypes.FirebaseAuthUserCreated, Services.FirebaseAuth) },
            { "providers/firebase.auth/eventTypes/user.delete",
                new FirebaseAuthEventAdapter(EventTypes.FirebaseAuthUserDeleted, Services.FirebaseAuth) },
            { "providers/google.firebase.analytics/eventTypes/event.log",
                new FirebaseAnalyticsEventAdapter(EventTypes.FirebaseAnalyticsLogWritten, Services.FirebaseAnalytics) },
            { "providers/google.firebase.database/eventTypes/ref.create",
                new FirebaseRtdbEventAdapter(EventTypes.FirebaseDatabaseDocumentCreated, Services.FirebaseDatabase) },
            { "providers/google.firebase.database/eventTypes/ref.write",
                new FirebaseRtdbEventAdapter(EventTypes.FirebaseDatabaseDocumentWritten, Services.FirebaseDatabase) },
            { "providers/google.firebase.database/eventTypes/ref.update",
                new FirebaseRtdbEventAdapter(EventTypes.FirebaseDatabaseDocumentUpdated, Services.FirebaseDatabase) },
            { "providers/google.firebase.database/eventTypes/ref.delete",
                new FirebaseRtdbEventAdapter(EventTypes.FirebaseDatabaseDocumentDeleted, Services.FirebaseDatabase) },
            { "providers/cloud.pubsub/eventTypes/topic.publish", new PubSubEventAdapter(EventTypes.PubSubMessagePublished) },
            { "providers/cloud.storage/eventTypes/object.change", new StorageEventAdapter(EventTypes.StorageObjectChanged) },
            { "google.firebase.remoteconfig.update", new EventAdapter(EventTypes.FirebaseRemoteConfigUpdated, Services.FirebaseRemoteConfig) },
        };

        internal static async Task<CloudEvent> ConvertGcfEventToCloudEvent(HttpRequest request, CloudEventFormatter formatter, ILogger logger)
        {
            var jsonRequest = await ParseRequest(request, logger);
            // Validated as not null or empty in ParseRequest
            string gcfType = jsonRequest.Context.Type!;
            if (!s_eventTypeMapping.TryGetValue(gcfType, out var eventAdapter))
            {
                throw new ConversionException($"Unexpected event type for function: '{gcfType}'");
            }
            return eventAdapter.ConvertToCloudEvent(jsonRequest, formatter);
        }

        private static async Task<Request> ParseRequest(HttpRequest request, ILogger logger)
        {
            Request parsedRequest;
            if (request.ContentType != JsonContentType)
            {
                throw new ConversionException("Unable to convert request to CloudEvent.");
            }
            try
            {
                var json = await new StreamReader(request.Body).ReadToEndAsync();
                parsedRequest = JsonSerializer.Deserialize<Request>(json) ?? throw new ConversionException("Request body parsed to null request");
            }
            catch (JsonException e)
            {
                // Note: the details of the original exception will be lost in the normal case (where it's just logged)
                // but keeping in the ConversionException is useful for debugging purposes.
                throw new ConversionException($"Error parsing GCF event: {e.Message}", e);
            }

            PubSubEventAdapter.NormalizeRawRequest(parsedRequest, request.Path.Value, logger);
            parsedRequest.NormalizeContext();
            if (parsedRequest.Data is null ||
                string.IsNullOrEmpty(parsedRequest.Context.Id) ||
                string.IsNullOrEmpty(parsedRequest.Context.Type))
            {
                throw new ConversionException("Event is malformed; does not contain a payload, or the event ID or type is missing.");
            }
            return parsedRequest;
        }

        private static string ValidateNotNullOrEmpty(string? value, string name) =>
            string.IsNullOrEmpty(value)
            ? throw new ConversionException($"Event contained no {name}")
            : value;

        /// <summary>
        /// An event-specific adapter, picked based on the event type from the GCF event.
        /// This class provides seams for event-specific changes to the data and the source/subject,
        /// but the defult implementations of the seams are reasonable for most events.
        /// </summary>
        private class EventAdapter
        {
            private readonly string _cloudEventType;
            private readonly string _defaultService;

            internal EventAdapter(string cloudEventType, string defaultService) =>
                (_cloudEventType, _defaultService) = (cloudEventType, defaultService);

            internal CloudEvent ConvertToCloudEvent(Request request, CloudEventFormatter formatter)
            {
                MaybeReshapeData(request);

                var context = request.Context;
                var resource = context.Resource;
                // Default the service name if necessary
                resource.Service = ValidateNotNullOrEmpty(resource.Service ?? _defaultService, "service");
                ValidateNotNullOrEmpty(resource.Name, "resource name");

                var evt = new CloudEvent
                {
                    Type = _cloudEventType,
                    Id = context.Id,
                    Time = context.Timestamp,
                    DataContentType = JsonContentType
                };
                PopulateAttributes(request, evt);
                var bytes = JsonSerializer.SerializeToUtf8Bytes(request.Data);
                formatter.DecodeBinaryModeEventData(bytes, evt);
                return evt;
            }

            protected virtual void MaybeReshapeData(Request request) { }

            /// <summary>
            /// Populate the attributes in the CloudEvent that can't always be determined directly.
            /// In particular, this method must populate the Source context attribute,
            /// should populate the Subject context attribute if it's defined for this event type,
            /// and should populate any additional extension attributes to be compatible with Eventarc.
            /// The resource within context of the request will definitely have its Service and Name properties populated.
            /// This base implementation populates the Source based on the service and resource.
            /// </summary>
            /// <param name="request">The incoming request.</param>
            /// <param name="evt">The event to populate</param>
            protected virtual void PopulateAttributes(Request request, CloudEvent evt)
            {
                var service = request.Context.Resource.Service!;
                var resource = request.Context.Resource.Name!;
                evt.Source = FormatSource(service, resource);
            }

            protected Uri FormatSource(string service, string resource) => new Uri($"//{service}/{resource}", UriKind.RelativeOrAbsolute);
        }

        private class StorageEventAdapter : EventAdapter
        {
            private static readonly CloudEventAttribute s_bucketAttribute = CloudEventAttribute.CreateExtension("bucket", CloudEventAttributeType.String);

            // Regex to split a storage resource name into bucket and object names.
            // Note the removal of "#" followed by trailing digits at the end of the object name;
            // this represents the object generation and should not be included in the CloudEvent subject.
            private static readonly Regex StorageResourcePattern =
                new Regex(@"^(?<resource>projects/_/buckets/(?<bucket>[^/]+))/(?<subject>objects/(?<object>.*?))(?:#\d+)?$", RegexOptions.Compiled);

            internal StorageEventAdapter(string cloudEventType) : base(cloudEventType, Services.Storage)
            {
            }

            /// <summary>
            /// Split a Storage object resource name into bucket (in the source) and object (in the subject)
            /// </summary>
            protected override void PopulateAttributes(Request request, CloudEvent evt)
            {
                var resource = request.Context.Resource.Name;
                if (StorageResourcePattern.Match(resource) is { Success: true } match)
                {
                    // The source includes the resource name up to the bucket
                    evt.Source = FormatSource(request.Context.Resource.Service!, match.Groups["resource"].Value);
                    // Resource name from "objects" onwards, but without the generation
                    evt.Subject = match.Groups["subject"].Value;
                    evt[s_bucketAttribute] = match.Groups["bucket"].Value;
                }
                else
                {
                    // If the resource name didn't match for whatever reason, just use the default behaviour.
                    base.PopulateAttributes(request, evt);
                }
            }
        }

        private class PubSubEventAdapter : EventAdapter
        {
            private static readonly CloudEventAttribute s_topicAttribute = CloudEventAttribute.CreateExtension("topic", CloudEventAttributeType.String);

            private static readonly Regex PubSubResourcePattern =
                new Regex(@"^projects/(?<project>[^/]+)/topics/(?<topic>[^/]+)$", RegexOptions.Compiled);

            internal PubSubEventAdapter(string cloudEventType) : base(cloudEventType, Services.PubSub)
            {
            }

            protected override void PopulateAttributes(Request request, CloudEvent evt)
            {
                base.PopulateAttributes(request, evt);
                var match = PubSubResourcePattern.Match(request.Context.Resource.Name);
                if (match.Success)
                {
                    evt[s_topicAttribute] = match.Groups["topic"].Value;
                }
            }

            /// <summary>
            /// Wrap the PubSub message and copy the message ID and publication time into
            /// it, for consistency with Eventarc.
            /// </summary>
            protected override void MaybeReshapeData(Request request)
            {
                if (request.Context.Id is string id)
                {
                    request.Data["messageId"] = id;
                }
                if (request.Context.Timestamp is DateTimeOffset timestamp)
                {
                    // Provide at least millisecond precision, and microsecond precision if sub-millisecond is provided in the input.
                    // (Currently only millisecond precision is provided, but this approach will be robust in the face of more precision.)
                    var formatString = timestamp.Ticks % 10_000 == 0
                        ? "yyyy-MM-dd'T'HH:mm:ss.fff'Z'"
                        : "yyyy-MM-dd'T'HH:mm:ss.ffffff'Z'";
                    request.Data["publishTime"] = timestamp.UtcDateTime.ToString(formatString, CultureInfo.InvariantCulture);
                }
                request.Data = new Dictionary<string, object> { { "message", request.Data } };

                // The subscription is not provided in the legacy format, but *is* provided in
                // the raw Pub/Sub push notification (including in the emulator) so we should use it if we've got it.
                if (request.RawPubSubSubscription is string subscription)
                {
                    request.Data["subscription"] = subscription;
                }
            }

            /// <summary>
            /// Normalizes a raw Pub/Sub push notification (from either the emulator
            /// or the real Pub/Sub service) into the existing legacy format.
            /// </summary>
            /// <param name="request">The incoming request body, parsed as a <see cref="Request"/> object</param>
            /// <param name="path">The HTTP request path, from which the topic name will be extracted.</param>
            /// <param name="logger">The logger to use for a warning if the path cannot be used to derive a topic.</param>
            internal static void NormalizeRawRequest(Request request, string path, ILogger logger)
            {
                // Non-raw-Pub/Sub path: just a no-op
                if (request.RawPubSubMessage is null && request.RawPubSubSubscription is null)
                {
                    return;
                }
                if (request.RawPubSubMessage is null || request.RawPubSubSubscription is null)
                {
                    throw new ConversionException("Request is malformed; it must contain both 'message' and 'subscription' properties or neither.");
                }
                if (request.Data is object ||
                    request.Domain is object ||
                    request.Context is object ||
                    request.EventId is object ||
                    request.EventType is object ||
                    request.Params is object ||
                    request.Timestamp is object)
                {
                    throw new ConversionException("Request is malformed; raw Pub/Sub request must contain only 'message' and 'subscription' properties.");
                }
                if (!request.RawPubSubMessage.TryGetValue("messageId", out var messageIdObj) || !(messageIdObj is JsonElement messageIdElement) ||
                    messageIdElement.ValueKind != JsonValueKind.String)
                {
                    throw new ConversionException("Request is malformed; raw Pub/Sub message must contain a 'message.messageId' string property.");
                }
                request.EventId = messageIdElement.GetString();
                request.EventType = GcfPubSubTopicPublishType;
                // Skip the leading / in the path
                path = path.Length == 0 ? "" : path.Substring(1);
                var topicPathMatch = PubSubResourcePattern.Match(path);
                if (topicPathMatch.Success)
                {
                    request.Resource = path;
                }
                else
                {
                    // TODO: Is it okay to log a warning on every request, or should we only log once (ever?)
                    logger.LogWarning("Request path did not represent a Pub/Sub topic name. The event will contain the unknown topic name of '{name}'", DefaultRawPubSubTopic);
                    request.Resource = DefaultRawPubSubTopic;
                }
                request.Data = request.RawPubSubMessage;
                if (request.RawPubSubMessage.TryGetValue("publishTime", out var publishTime) &&
                    publishTime is JsonElement publishTimeElement &&
                    publishTimeElement.ValueKind == JsonValueKind.String &&
                    DateTimeOffset.TryParseExact(publishTimeElement.GetString(), "yyyy-MM-dd'T'HH:mm:ss.FFFFFF'Z'", CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                        out var timestamp))
                {
                    request.Timestamp = timestamp;
                }
            }
        }

        /// <summary>
        /// Adapter for Firestore events that need extra source/subject handling.
        /// The resource names for Firestore have "documents" as the fifth segment.
        /// For example:
        /// "projects/project-id/databases/(default)/documents/gcf-test/IH75dRdeYJKd4uuQiqch".
        /// We validate that the fifth segment has the value we expect, and then use that segment onwards as the subject.
        /// </summary>
        private class FirestoreDocumentEventAdapter : EventAdapter
        {
            private const int SubjectStartIndex = 4; // The subject always starts at the 5th segment
            private const string ExpectedSubjectPrefix = "documents";

            internal FirestoreDocumentEventAdapter(string cloudEventType, string defaultService)
                : base(cloudEventType, defaultService)
            {
            }

            // Reformat the service/resource so that the part of the resource before "documents" is in the source, but
            // "documents" onwards is in the subject.
            protected override void PopulateAttributes(Request request, CloudEvent evt)
            {
                string resource = request.Context.Resource.Name!;
                string service = request.Context.Resource.Service!;

                string[] resourceSegments = resource.Split('/', SubjectStartIndex + 1);
                if (resourceSegments.Length < SubjectStartIndex ||
                    !resourceSegments[SubjectStartIndex].StartsWith(ExpectedSubjectPrefix, StringComparison.Ordinal))
                {
                    // This is odd... just put everything in the source as normal.
                    base.PopulateAttributes(request, evt);
                }
                else
                {
                    string sourcePath = string.Join('/', resourceSegments.Take(SubjectStartIndex));
                    evt.Source = FormatSource(service, sourcePath);
                    evt.Subject = resourceSegments[SubjectStartIndex];
                }
            }
        }

        /// <summary>
        /// Adapter for Firebase RTDB events that need extra source/subject handling.
        /// The resource names for Firebase have "refs" as the fifth segment.
        /// For example:
        /// "projects/_/instances/my-project-id/refs/gcf-test/xyz" or
        /// We validate that the fifth segment has the value we expect, and then use that segment onwards as the subject.
        /// Additionally, we add the location, derived from the "domain" part of the context, into the source
        /// </summary>
        private class FirebaseRtdbEventAdapter : EventAdapter
        {
            private const int SubjectStartIndex = 4; // The subject always starts at the 5th segment
            private const string ExpectedSubjectPrefix = "refs";
            private const string FirebaseDefaultDomain = "firebaseio.com";
            private const string FirebaseDefaultLocation = "us-central1";

            internal FirebaseRtdbEventAdapter(string cloudEventType, string defaultService)
                : base(cloudEventType, defaultService)
            {
            }

            // Reformat the service/resource so that the part of the resource before "refs" is in the source,
            // with the addition of the location, but "refs" onwards is in the subject.
            protected override void PopulateAttributes(Request request, CloudEvent evt)
            {
                string resource = request.Context.Resource.Name!;
                string service = request.Context.Resource.Service!;

                string? domain = request.Domain;
                if (string.IsNullOrEmpty(domain))
                {
                    throw new ConversionException("Firebase RTDB event does not contain a domain");
                }
                string location = domain == FirebaseDefaultDomain
                    ? FirebaseDefaultLocation
                    : domain.Split('.').First();

                string[] resourceSegments = resource.Split('/', SubjectStartIndex + 1);
                if (resourceSegments.Length < SubjectStartIndex ||
                    !resourceSegments[SubjectStartIndex].StartsWith(ExpectedSubjectPrefix, StringComparison.Ordinal))
                {
                    // This is odd... just put everything in the source as normal.
                    base.PopulateAttributes(request, evt);
                }
                else
                {
                    // We need to convert "projects/_/instances/{instance}" to "projects/_/locations/{location}/instances/{instance}".
                    string sourcePath = string.Join('/',
                        resourceSegments.Take(2).Concat(new[] { "locations", location }).Concat(resourceSegments.Skip(2).Take(2)));
                    evt.Subject = resourceSegments[SubjectStartIndex];
                    evt.Source = FormatSource(service, sourcePath);
                }
            }
        }

        private class FirebaseAuthEventAdapter : EventAdapter
        {
            internal FirebaseAuthEventAdapter(string cloudEventType, string defaultService)
                : base(cloudEventType, defaultService)
            {
            }

            protected override void PopulateAttributes(Request request, CloudEvent evt)
            {
                // Source is as normal
                base.PopulateAttributes(request, evt);
                // Obtain the subject from the data if possible.
                if (request.Data.TryGetValue("uid", out var uid) && uid is JsonElement uidElement && uidElement.ValueKind == JsonValueKind.String)
                {
                    evt.Subject = $"users/{uidElement.GetString()}";
                }
            }

            /// <summary>
            /// Rename fields within metadata: lastSignedInAt => lastSignInTime, and createdAt => createTime.
            /// </summary>
            /// <param name="request"></param>
            protected override void MaybeReshapeData(Request request)
            {
                if (request.Data.TryGetValue("metadata", out var metadata) &&
                    metadata is JsonElement metadataElement &&
                    metadataElement.ValueKind == JsonValueKind.Object)
                {
                    // JsonElement itself is read-only, but we can just replace the value with a
                    // Dictionary<string, JsonElement> that has the appropriate key/value pairs.
                    var metadataDict = new Dictionary<string, JsonElement>();
                    foreach (var item in metadataElement.EnumerateObject())
                    {
                        string name = item.Name switch
                        {
                            "lastSignedInAt" => "lastSignInTime",
                            "createdAt" => "createTime",
                            var x => x
                        };
                        metadataDict[name] = item.Value;
                    }
                    request.Data["metadata"] = metadataDict;
                }
            }
        }

        private class FirebaseAnalyticsEventAdapter : EventAdapter
        {
            internal FirebaseAnalyticsEventAdapter(string cloudEventType, string defaultService)
                : base(cloudEventType, defaultService)
            {
            }

            protected override void PopulateAttributes(Request request, CloudEvent evt)
            {
                // We need the event name and the app ID in order to create subject and source like this:
                // Subject://firebaseanalytics.googleapis.com/projects/{project}/apps/{app}
                // Source: events/{eventName}
                // The eventName is available in the resource, which is of the form projects/{project-id}/events/{event-name}
                // The app name is available via the data, via userDim -> appInfo -> appId
                // If either of these isn't where we expect, we throw a ConversionException
                string resource = request.Context.Resource.Name!;
                string service = request.Context.Resource.Service!;
                var data = request.Data;

                string[] splitResource = resource.Split('/');
                string? eventName = splitResource.Length == 4 && splitResource[0] == "projects" && splitResource[2] == "events" ? splitResource[3] : null;
                string? appId = data.TryGetValue("userDim", out var userDim) && userDim is JsonElement userDimElement &&
                    userDimElement.TryGetProperty("appInfo", out var appInfoElement) &&
                    appInfoElement.TryGetProperty("appId", out var appIdElement) &&
                    appIdElement.ValueKind == JsonValueKind.String
                    ? appIdElement.GetString() : null;

                if (eventName is object && appId is object)
                {
                    string projectId = splitResource[1]; // Definitely valid, given that we got the event name.
                    evt.Source = new Uri($"//{service}/projects/{projectId}/apps/{appId}", UriKind.RelativeOrAbsolute);
                    evt.Subject = $"events/{eventName}";
                }
                else
                {
                    throw new ConversionException("Firebase Analytics event does not contain expected data");
                }
            }
        }

        /// <summary>
        /// Exception thrown to indicate that the conversion of an HTTP request
        /// into a CloudEvent has failed. This is handled within <see cref="CloudEventAdapter{TData}"></see> and <see cref="CloudEventAdapter"/>.
        /// </summary>
        internal sealed class ConversionException : Exception
        {
            internal ConversionException(string message) : base(message)
            {
            }

            internal ConversionException(string message, Exception innerException) : base(message, innerException)
            {
            }
        }
    }
}
