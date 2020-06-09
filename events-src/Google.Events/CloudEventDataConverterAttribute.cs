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

namespace Google.Events
{
    /// <summary>
    /// Indicates the converter type for the "target" type on which this attribute is placed.
    /// The converter type is expected to be a concrete type implementing
    /// <see cref="ICloudEventDataConverter{T}"/> for the target type, and must have a public
    /// parameterless constructor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class CloudEventDataConverterAttribute : Attribute
    {
        /// <summary>
        /// The type performing the data conversions.
        /// </summary>
        public Type ConverterType { get; }

        /// <summary>
        /// Constructs an instance of the attribute for the specified converter type.
        /// </summary>
        /// <param name="converterType">The type performing the data conversions.</param>
        public CloudEventDataConverterAttribute(Type converterType) =>
            ConverterType = converterType;
    }
}
