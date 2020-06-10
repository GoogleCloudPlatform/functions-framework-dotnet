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

using Google.Protobuf.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

// This file contains manual additions to the code generated from the protos.

namespace Google.Events.Protobuf.Cloud.Firestore.V1
{
    public partial class Document
    {
        /// <summary>
        /// Converts the fields in <see cref="Fields"/> into an idiomatic .NET representation.
        /// </summary>
        public IReadOnlyDictionary<string, object> ConvertFields() => ConvertMap(Fields);

        private static IReadOnlyDictionary<string, object> ConvertMap(MapField<string, Value> fields)
        {
            var dictionary = fields.ToDictionary(pair => pair.Key, pair => ConvertValue(pair.Value));
            return new ReadOnlyDictionary<string, object>(dictionary);
        }

        private static object ConvertValue(Value value) => value.ValueTypeCase switch
        {
            Value.ValueTypeOneofCase.ArrayValue => value.ArrayValue.Values.Select(ConvertValue).ToList().AsReadOnly(),
            Value.ValueTypeOneofCase.BooleanValue => value.BooleanValue,
            Value.ValueTypeOneofCase.DoubleValue => value.DoubleValue,
            Value.ValueTypeOneofCase.GeoPointValue => value.GeoPointValue,
            Value.ValueTypeOneofCase.IntegerValue => value.IntegerValue,
            Value.ValueTypeOneofCase.MapValue => ConvertMap(value.MapValue.Fields),
            Value.ValueTypeOneofCase.NullValue => null,
            Value.ValueTypeOneofCase.ReferenceValue => value.ReferenceValue,
            Value.ValueTypeOneofCase.StringValue => value.StringValue,
            Value.ValueTypeOneofCase.TimestampValue => value.TimestampValue.ToDateTimeOffset(),
            Value.ValueTypeOneofCase.BytesValue => value.BytesValue.ToByteArray(),
            // FIXME: This shouldn't be necessary, but we don't parse (or format) the JSON value of NullValue correctly at the moment.
            Value.ValueTypeOneofCase.None => null,
            _ => throw new ArgumentException($"Invalid value case: {value.ValueTypeCase}")
        };
    }
}
