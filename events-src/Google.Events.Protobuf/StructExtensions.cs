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

using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace Google.Events.Protobuf
{
    /// <summary>
    /// Extensions to make <see cref="Google.Protobuf.WellKnownTypes.Struct"/> easier to work with.
    /// Eventually this functionality may become part of the support library.
    /// </summary>
    public static class StructExtensions
    {
        private static readonly char[] s_slashSplit = { '/' };

        /// <summary>
        /// Returns the value of a field within the given struct, traversing the given path.
        /// </summary>
        /// <param name="struct">The struct to find the value in. Must not be null.</param>
        /// <param name="path">The path to traverse, e.g. <c>foo/bar[3]/baz</c>. Must not be null.</param>
        /// <returns>The field for the path, converted to a suitable .NET type.</returns>
        public static object GetValue(this Struct @struct, string path)
        {
            ProtoPreconditions.CheckNotNull(@struct, nameof(@struct));
            ProtoPreconditions.CheckNotNull(path, nameof(path));
            string[] segments = path.Split(s_slashSplit, StringSplitOptions.None);
            Struct currentStruct = @struct;
            Value currentValue = null; // We always go through the loop at least once, so this is okay.
            foreach (var segment in segments)
            {
                if (currentStruct is null)
                {
                    throw new ArgumentException($"Path segment '{segment}' in path occurred when not in a struct value");
                }
                int indexStart = segment.IndexOf('[');
                int indexEnd = segment.IndexOf(']');
                // List handling
                if (indexStart != -1 || indexEnd != -1)
                {
                    // There must be exactly one [, exactly one ], and
                    // the [ must come before ].
                    if (indexStart == -1 || indexEnd == -1 ||
                        segment.IndexOf('[', indexStart + 1) != -1 ||
                        segment.IndexOf(']', indexEnd + 1) != -1 ||
                        indexStart > indexEnd)
                    {
                        throw new ArgumentException($"Path segment '{segment}' is invalid", nameof(path));
                    }
                    string indexText = segment.Substring(indexStart + 1, indexEnd - indexStart - 1);
                    if (!int.TryParse(indexText, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out int index))
                    {
                        throw new ArgumentException($"Index part of path segment '{segment}' cannot be parsed", nameof(path));
                    }
                    string fieldName = segment.Substring(0, indexStart);
                    if (!currentStruct.Fields.TryGetValue(fieldName, out currentValue))
                    {
                        throw new ArgumentException($"Field '{fieldName}' from path segment '{segment}' not found within parent");
                    }
                    if (currentValue.KindCase != Value.KindOneofCase.ListValue)
                    {
                        throw new ArgumentException($"Field '{fieldName}' from path segment '{segment}' does not represent a list");
                    }
                    currentValue = currentValue.ListValue.Values[index];
                }
                // Non-array handling
                else if (!currentStruct.Fields.TryGetValue(segment, out currentValue))
                {
                    throw new ArgumentException($"Path segment '{segment}' not found within parent");
                }
                currentStruct = currentValue.StructValue;
            }
            return ConvertValue(currentValue);
        }

        // TODO: Maybe make this public in a ValueExtensions class?
        private static object ConvertValue(Value value) => value.KindCase switch
        {
            Value.KindOneofCase.ListValue => value.ListValue.Values.Select(ConvertValue).ToList().AsReadOnly(),
            Value.KindOneofCase.BoolValue => value.BoolValue,
            Value.KindOneofCase.NumberValue => value.NumberValue,
            Value.KindOneofCase.StringValue => value.StringValue,
            Value.KindOneofCase.StructValue =>
                new ReadOnlyDictionary<string, object>(value.StructValue.Fields.ToDictionary(pair => pair.Key, pair => ConvertValue(pair.Value))),
            Value.KindOneofCase.NullValue => null,
            _ => throw new ArgumentException($"Invalid value case: {value.KindCase}")
        };
    }
}
