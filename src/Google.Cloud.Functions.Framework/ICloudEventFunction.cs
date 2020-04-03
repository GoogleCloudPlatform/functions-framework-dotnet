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
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Google.Cloud.Functions.Framework
{
    /// <summary>
    /// Function accepting a Cloud Event.
    /// </summary>
    public interface ICloudEventFunction
    {
        /// <summary>
        /// Asynchronously handles the specified Cloud Event.
        /// </summary>
        /// <param name="cloudEvent">The Cloud Event extracted from the request.</param>
        /// <returns>A task representing the potentially-asynchronous handling of the event.
        /// If the task completes, the function is deemed to be successful.</returns>
        Task HandleAsync(CloudEvent cloudEvent);
    }
}
