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
using System;

namespace Google.Events
{
    /// <summary>
    /// Defines methods for converting the data within a CloudEvent to a .NET object.
    /// Implementations are expected to be thread-safe and stateless.
    /// This is typically the type decorated with the <see cref="CloudEventDataConverterAttribute"/> attribute;
    /// see the attribute documentation for additional expectations when the converter is discovered via the
    /// attribute.
    /// </summary>
    /// <typeparam name="T">The type to deserialize CloudEvent data to.</typeparam>
    public interface ICloudEventDataConverter<T> where T : class
    {
        /// <summary>
        /// Converts the data in <paramref name="cloudEvent"/> to the type specified at construction
        /// time.
        /// </summary>
        /// <param name="cloudEvent">The CloudEvent to obtain data from. This may be used to check
        /// the data content type as well. It should not be mutated by this method.</param>
        /// <returns>The data from the CloudEvent, deserialized to <typeparamref name="T"/>.</returns>
        T ConvertEventData(CloudEvent cloudEvent);

        /// <summary>
        /// Populates <paramref name="cloudEvent"/> with the data in <paramref name="data"/>.
        /// </summary>
        /// <remarks>
        /// If <see cref="CloudEvent.DataContentType"/> is set within <paramref name="cloudEvent"/>, this
        /// must not be changed, and should reflect the content type to use within the event. If no content
        /// type has been specified, the converter should use its preferred content type and set the
        /// <see cref="CloudEvent.DataContentType"/> property accordingly.
        /// </remarks>
        /// <param name="cloudEvent">The CloudEvent to populate. Must not be null. When
        /// the method completes, the <see cref="CloudEvent.Data"/> and <see cref="CloudEvent.DataContentType"/>
        /// properties should be set.</param>
        /// <param name="data">The data to populate within the CloudEvent.</param>
        /// <exception cref="ArgumentException">The content type specified within <paramref name="cloudEvent"/> cannot be provided
        /// by this converter.</exception>
        void PopulateCloudEvent(CloudEvent cloudEvent, T data);
    }
}
