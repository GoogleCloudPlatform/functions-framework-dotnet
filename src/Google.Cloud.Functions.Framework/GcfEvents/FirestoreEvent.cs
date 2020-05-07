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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Google.Cloud.Functions.Framework.GcfEvents
{
    /// <summary>
    /// The CloudEvent representation of a Firestore event as translated from a GCF event.
    /// </summary>
    public sealed class FirestoreEvent
    {
        /// <summary>
        /// A <see cref="FirestoreDocument"/> containing a post-operation document snapshot.
        /// </summary>
        [JsonPropertyName("value")]
        public FirestoreDocument? Value { get; set; }

        /// <summary>
        /// For update and delete options, a <see cref="FirestoreDocument"/> containing a pre-operation document snapshot
        /// </summary>
        [JsonPropertyName("oldValue")]
        public FirestoreDocument? OldValue { get; set; }

        /// <summary>
        /// For update operations, a <see cref="FirestoreDocumentMask"/> that lists
        /// changed fields.
        /// </summary>
        [JsonPropertyName("updateMask")]
        public FirestoreDocumentMask? UpdateMask { get; set; }

        /// <summary>
        /// The wildcards matched within the trigger resource name. For example, with a trigger resource name
        /// ending in "documents/players/{player}/levels/{level}", matching a document with a resource name ending in
        /// "documents/players/player1/levels/level1", the mapping would be from "player" to "player1" and "level" to "level1".
        /// </summary>
        [JsonPropertyName("wildcards")]
        public IDictionary<string, string> Wildcards { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// A Firestore document.
    /// </summary>
    public sealed class FirestoreDocument
    {
        /// <summary>
        /// The resource name of the document.
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// The time at which the document was created.
        /// </summary>
        [JsonPropertyName("createTime")]
        public DateTimeOffset? CreateTime { get; set; }

        /// <summary>
        /// The time at which the document was last changed.
        /// </summary>
        [JsonPropertyName("updateTime")]
        public DateTimeOffset? UpdateTime { get; set; }

        /// <summary>
        /// The document's fields.
        /// </summary>
        [JsonPropertyName("fields")]
        [JsonConverter(typeof(FirestoreFieldMapConverter))]
        public IDictionary<string, object?>? Fields { get; set; }
    }

    /// <summary>
    /// An object representing a latitude/longitude pair.
    /// </summary>
    public sealed class FirestoreGeoPoint
    {
        /// <summary>
        /// The latitude in degrees, in the range [-90.0, +90.0].
        /// </summary>
        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        /// <summary>
        /// The longitude in degrees, in the range [-180.0, +180.0].
        /// </summary>
        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }
    }

    /// <summary>
    /// A document mask used to list changed fields within a document.
    /// </summary>
    public sealed class FirestoreDocumentMask
    {
        /// <summary>
        /// The set of field paths that were updated in the document.
        /// </summary>
        [JsonPropertyName("fieldPaths")]
        public IList<string>? FieldList { get; set; }
    }

    // The rest of this file is internal versions - basically allowing users to avoid the
    // complex representation of Firestore documents.

    internal sealed class FirestoreFieldMapConverter : JsonConverter<IDictionary<string, object?>>
    {
        public override IDictionary<string, object?> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var map = JsonSerializer.Deserialize<IDictionary<string, FirestoreValue>>(ref reader);
            return map.ToDictionary(pair => pair.Key, pair => pair.Value?.Value);
        }

        public override void Write(Utf8JsonWriter writer, IDictionary<string, object?> value, JsonSerializerOptions options) =>
            throw new NotImplementedException("FirestoreDocument serialization is not currently supported");
    }

    internal sealed class FirestoreMapValueConverter : JsonConverter<IDictionary<string, object?>>
    {
        public override IDictionary<string, object?> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var map = JsonSerializer.Deserialize<FirestoreMapValue>(ref reader);
            return map.Fields!.ToDictionary(pair => pair.Key, pair => pair.Value?.Value);
        }

        public override void Write(Utf8JsonWriter writer, IDictionary<string, object?> value, JsonSerializerOptions options) =>
            throw new NotImplementedException("FirestoreDocument serialization is not currently supported");
    }

    internal sealed class FirestoreArrayValueConverter : JsonConverter<IList<object?>>
    {
        public override IList<object?> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var map = JsonSerializer.Deserialize<FirestoreArrayValue>(ref reader);
            return map.Values!.Select(value => value.Value).ToList();
        }

        public override void Write(Utf8JsonWriter writer, IList<object?> value, JsonSerializerOptions options) =>
            throw new NotImplementedException("FirestoreDocument serialization is not currently supported");
    }

    internal sealed class FirestoreValue
    {
        private Case _case;

        public object? Value { get; private set; }

        internal enum Case
        {
            Null,
            Boolean,
            Integer,
            Double,
            Timestamp,
            String,
            Bytes,
            DocumentReference,
            GeoPoint,
            Array,
            Map
        }

        private void SetValue(Case @case, object? value)
        {
            _case = @case;
            Value = value;
        }

        [JsonPropertyName("nullValue")]
        public string? NullValue
        {
            get => null;
            set => SetValue(Case.Null, null);
        }

        [JsonPropertyName("booleanValue")]
        public bool BoolValue
        {
            get => (bool) Value!;
            set => SetValue(Case.Boolean, value);
        }

        [JsonPropertyName("integerValue")]
        [JsonConverter(typeof(Int64FromStringConverter))]
        public long IntegerValue
        {
            get => (long) Value!;
            set => SetValue(Case.Integer, value);
        }

        [JsonPropertyName("doubleValue")]
        public double DoubleValue
        {
            get => (double) Value!;
            set => SetValue(Case.Double, value);
        }

        [JsonPropertyName("timestampValue")]
        public DateTimeOffset TimestampValue
        {
            get => (DateTimeOffset) Value!;
            set => SetValue(Case.Timestamp, value);
        }

        [JsonPropertyName("stringValue")]
        public string StringValue
        {
            get => (string) Value!;
            set => SetValue(Case.String, value);
        }

        [JsonPropertyName("bytesValue")]
        public byte[] BytesValue
        {
            get => (byte[]) Value!;
            set => SetValue(Case.Bytes, value);
        }

        [JsonPropertyName("referenceValue")]
        public string DocumentReferenceValue
        {
            get => (string) Value!;
            set => SetValue(Case.DocumentReference, value);
        }

        [JsonPropertyName("geoPointValue")]
        public FirestoreGeoPoint GeoPointValaue
        {
            get => (FirestoreGeoPoint) Value!;
            set => SetValue(Case.GeoPoint, value);
        }

        [JsonPropertyName("arrayValue")]
        [JsonConverter(typeof(FirestoreArrayValueConverter))]
        public IList<object?> ArrayValue
        {
            get => (IList<object?>) Value!;
            set => SetValue(Case.Array, value);
        }

        [JsonPropertyName("mapValue")]
        [JsonConverter(typeof(FirestoreMapValueConverter))]
        public IDictionary<string, object?> MapValue
        {
            get => (IDictionary<string, object?>) Value!;
            set => SetValue(Case.Array, value);
        }
    }

    internal sealed class FirestoreArrayValue
    {
        [JsonPropertyName("values")]
        public IList<FirestoreValue>? Values { get; set; }
    }

    internal sealed class FirestoreMapValue
    {
        [JsonPropertyName("fields")]
        public IDictionary<string, FirestoreValue>? Fields { get; set; }
    }
}
