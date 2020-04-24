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

using System.Threading;
using System.Threading.Tasks;

namespace Google.Cloud.Functions.Framework.LegacyEvents
{
    /// <summary>
    /// A function to handle a "legacy" event.
    /// </summary>
    /// <typeparam name="T">The type of the event payload. This is typically <see cref="StorageObject"/>,
    /// <see cref="PubSubMessage"/> or <see cref="FirestoreEvent"/>, but can be any class that can be
    /// deserialized from the "data" field of a legacy event.
    /// </typeparam>
    public interface ILegacyEventFunction<T> where T : class
    {
        /// <summary>
        /// Asynchronously handles the specified legacy event.
        /// </summary>
        /// <param name="payload">The payload of the event.</param>
        /// <param name="context">The context of the event.</param>
        /// <param name="cancellationToken">A cancellation token which indicates if the request is aborted.</param>
        /// <returns>A task representing the potentially-asynchronous handling of the event.
        /// If the task completes, the function is deemed to be successful.</returns>
        Task HandleAsync(T payload, Context context, CancellationToken cancellationToken);
    }
}
