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
using System.Collections.Concurrent;
using System.Reflection;

namespace Google.Events
{
    /// <summary>
    /// Utility class to obtain and execute converters by reflecting over attributes applied to
    /// the target types, caching converters appropriately.
    /// </summary>
    public static class CloudEventConverters
    {
        private static readonly ConcurrentDictionary<Type, object> s_dataConverters =
            new ConcurrentDictionary<Type, object>();

        // Internal for testing. (We may want to make it public at some point, in which case we'd probably want to
        // think about the name first.)
        internal static ICloudEventDataConverter<T> GetDataConverter<T>() where T : class =>
            (ICloudEventDataConverter<T>) s_dataConverters.GetOrAdd(typeof(T), type => CreateDataConverter<T>());

        private static ICloudEventDataConverter<T> CreateDataConverter<T>() where T : class
        {
            try
            {
                var targetType = typeof(T);
                var attribute = targetType.GetCustomAttribute<CloudEventDataConverterAttribute>(true);
                // It's fine for the converter creation to fail: we'll try it on every attempt,
                // and always end up with an exception.
                if (attribute is null)
                {
                    throw new ArgumentException($"Type {targetType} does not have {nameof(CloudEventDataConverterAttribute)} applied to it.");
                }
                var converterType = attribute.ConverterType;
                if (converterType is null)
                {
                    throw new ArgumentException($"The {nameof(CloudEventDataConverterAttribute)} on type {targetType} has no converter type specified.");
                }

                var instance = Activator.CreateInstance(converterType);

                var converter = instance as ICloudEventDataConverter<T>;
                if (converter is null)
                {
                    throw new ArgumentException($"Data converter type {converterType} does not implement {nameof(ICloudEventDataConverter<T>)} with a target type of {targetType}");
                }

                return converter;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Unable to create data converter for target type {typeof(T)}", e);
            }
        }

        /// <summary>
        /// Converts the data in <paramref name="cloudEvent"/> into the type specified by <typeparamref name="T"/>,
        /// obtaining the <see cref="ICloudEventDataConverter{T}"/> associated with the type via <see cref="CloudEventDataConverterAttribute"/>.
        /// </summary>
        /// <typeparam name="T">The type to convert the event data to.</typeparam>
        /// <param name="cloudEvent">The CloudEvent to extract the data from. Must not be null.</param>
        /// <returns>The converted data.</returns>
        public static T ConvertCloudEventData<T>(CloudEvent cloudEvent) where T : class
        {
            EventsPreconditions.CheckNotNull(cloudEvent, nameof(cloudEvent));
            var converter = GetDataConverter<T>();
            return converter.ConvertEventData(cloudEvent);
        }


        /// <summary>
        /// Populates <paramref name="cloudEvent"/> with the data in <paramref name="data"/> by
        /// using the <see cref="ICloudEventDataConverter{T}"/> associated with <typeparamref name="T"/> via
        /// <see cref="CloudEventDataConverterAttribute"/>. See
        /// <see cref="ICloudEventDataConverter{T}.PopulateCloudEvent(CloudEvent, T)"/> for details of what gets populated.
        /// </summary>
        /// <param name="cloudEvent">The event to populate with data. Must not be null.</param>
        /// <param name="data">The data to populate within the event. May be null if the converter supports that.</param>
        public static void PopulateCloudEvent<T>(CloudEvent cloudEvent, T data) where T : class
        {
            EventsPreconditions.CheckNotNull(cloudEvent, nameof(cloudEvent));
            var converter = GetDataConverter<T>();
            converter.PopulateCloudEvent(cloudEvent, data);
        }
    }
}
