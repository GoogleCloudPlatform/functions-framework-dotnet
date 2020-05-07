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
using System.Threading;
using System.Threading.Tasks;

namespace Google.Cloud.Functions.Framework
{
    /// <summary>
    /// Function accepting a Cloud Event without performing any kind of data deserialization.
    /// </summary>
    public interface ICloudEventFunction
    {
        /// <summary>
        /// Asynchronously handles the specified Cloud Event.
        /// </summary>
        /// <param name="cloudEvent">The Cloud Event extracted from the request.</param>
        /// <param name="cancellationToken">A cancellation token which indicates if the request is aborted.</param>
        /// <returns>A task representing the potentially-asynchronous handling of the event.
        /// If the task completes, the function is deemed to be successful.</returns>
        Task HandleAsync(CloudEvent cloudEvent, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Function accepting a Cloud Event expecting a particular data type, which is automatically deserialized
    /// before function invocation.
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public interface ICloudEventFunction<TData> where TData : class
    {
        /// <summary>
        /// Asynchronously handles the specified Cloud Event.
        /// </summary>
        /// <param name="cloudEvent">The original Cloud Event extracted from the request.</param>
        /// <param name="data">The deserialized object constructed from the data.</param>
        /// <param name="cancellationToken">A cancellation token which indicates if the request is aborted.</param>
        /// <returns>A task representing the potentially-asynchronous handling of the event.
        /// If the task completes, the function is deemed to be successful.</returns>
        Task HandleAsync(CloudEvent cloudEvent, TData data, CancellationToken cancellationToken);
    }
}
