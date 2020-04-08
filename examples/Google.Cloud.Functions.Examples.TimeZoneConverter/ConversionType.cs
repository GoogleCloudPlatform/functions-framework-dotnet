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

namespace Google.Cloud.Functions.Examples.TimeZoneConverter
{
    /// <summary>
    /// The type of conversion that was performed as part of a <see cref="ConversionResult"/>.
    /// </summary>
    public enum ConversionType
    {
        /// <summary>
        /// The input was unambiguous, mapping to a single instant in time.
        /// </summary>
        Unambiguous,

        /// <summary>
        /// The input was ambiguous, usually due to occurring within a "fall back" daylight saving transition.
        /// The conversion to the target time zone of each value did not resolve this ambiguity.
        /// The result is the earlier of the results.
        /// </summary>
        AmbiguousInputAmbiguousResult,

        /// <summary>
        /// The input was ambiguous, usually due to occurring within a "fall back" daylight saving transition.
        /// However, after converting both possible instants to the target time zone, the results are the same.
        /// This is usually due to converting between time zones which observe the same daylight saving transitions.
        /// </summary>
        AmbiguousInputUnambiguousResult,

        /// <summary>
        /// The input was skipped, usually due to occurring within a "spring forward" daylight saving transition.
        /// The result is provided by shifting the input value by the length of the "gap" in local time (usually one hour).
        /// </summary>
        SkippedInputForwardShiftedResult
    }
}
