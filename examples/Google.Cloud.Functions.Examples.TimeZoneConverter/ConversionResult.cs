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

using System.Text.Json.Serialization;

namespace Google.Cloud.Functions.Examples.TimeZoneConverter
{
    // Note: Input and Result could be LocalDateTime values, if we use NodaTime.Serialization.SystemTextJson.
    // But we've already got a LocalDateTimePattern available, so it's simplest just to format with that.

    /// <summary>
    /// The result of a zone-to-zone conversion.
    /// </summary>
    public sealed class ConversionResult
    {
        /// <summary>
        /// The version of the data used, e.g. "TZDB: 2019b (mapping: 14742)".
        /// </summary>
        [JsonPropertyName("data_version")]
        public string DataVersion { get; }

        /// <summary>
        /// The local date/time used as input and interpreted in <see cref="FromZone"/>.
        /// </summary>
        [JsonPropertyName("input")]
        public string Input { get; }

        /// <summary>
        /// The ID of the time zone to convert from.
        /// </summary>
        [JsonPropertyName("from_zone")]
        public string FromZone { get; }

        /// <summary>
        /// The ID of the time zone to convert to.
        /// </summary>
        [JsonPropertyName("to_zone")]
        public string ToZone { get; }

        /// <summary>
        /// The result date/time in <see cref="ToZone"/>.
        /// </summary>
        [JsonPropertyName("result")]
        public string Result { get; }

        /// <summary>
        /// The conversion type. While most local date/time values map 1:1 with instants in time,
        /// daylight saving transitions and other offset changes mean that some local date/time values
        /// are skipped entirely or occur twice.
        /// </summary>
        [JsonPropertyName("conversion_type")]
        public ConversionType ConversionType { get; }

        public ConversionResult(string dataVersion, string input, string fromZone, string toZone, string result, ConversionType conversionType) =>
            (DataVersion, Input, FromZone, ToZone, Result, ConversionType) = (dataVersion, input, fromZone, toZone, result, conversionType);
    }
}
