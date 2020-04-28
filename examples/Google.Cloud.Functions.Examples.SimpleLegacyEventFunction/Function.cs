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

using Google.Cloud.Functions.Framework.LegacyEvents;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Google.Cloud.Functions.Examples.SimpleLegacyEventFunction
{
    /// <summary>
    /// A function that can be triggered in responses to changes in Google Cloud Storage.
    /// The type argument (StorageObject in this case) determines how the event payload is deserialized.
    /// The event must be deployed so that the trigger matches the expected payload type. (For example,
    /// deploying a function expecting a StorageObject payload will not work for a trigger that provides
    /// a FirestoreEvent.)
    /// </summary>
    public class Function : ILegacyEventFunction<StorageObject>
    {
        /// <summary>
        /// Logic for your function goes here. Note that a Legacy Event function just consumes an event;
        /// it doesn't provide any response.
        /// </summary>
        /// <param name="payload">The Cloud Event your function should respond to.</param>
        /// <param name="context">The Cloud Event your function should respond to.</param>
        /// <param name="cancellationToken">A cancellation token that is notified if the request is aborted.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task HandleAsync(StorageObject payload, Context context, CancellationToken cancellationToken)
        {
            Console.WriteLine("Context:");
            Console.WriteLine($"  ID: {context.Id}");
            Console.WriteLine($"  Type: {context.Type}");
            Console.WriteLine($"  Timestamp: {context.Timestamp:yyyy-MM-dd'T'HH:mm:ss.fff'Z'}");
            Console.WriteLine($"  Resource name: {context.Resource.Name}");
            Console.WriteLine($"  Resource service: {context.Resource.Service}");
            Console.WriteLine($"  Resource type: {context.Resource.Type}");
            Console.WriteLine("Storage object:");
            Console.WriteLine($"  Name: {payload.Name}");
            Console.WriteLine($"  Bucket: {payload.Bucket}");
            Console.WriteLine($"  Size: {payload.Size}");
            Console.WriteLine($"  Content type: {payload.ContentType}");

            // In this example, we don't need to perform any asynchronous operations, so the
            // method doesn't need to be declared async.
            return Task.CompletedTask;
        }
    }
}
