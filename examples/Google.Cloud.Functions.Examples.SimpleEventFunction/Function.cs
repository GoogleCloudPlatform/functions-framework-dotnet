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
using Google.Cloud.Functions.Framework;
using Google.Events.Protobuf.Cloud.Storage.V1;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Google.Cloud.Functions.Examples.SimpleEventFunction
{
    /// <summary>
    /// A function that can be triggered in responses to changes in Google Cloud Storage.
    /// The type argument (StorageObjectData in this case) determines how the event payload is deserialized.
    /// The function must be deployed so that the trigger matches the expected payload type. (For example,
    /// deploying a function expecting a StorageObject payload will not work for a trigger that provides
    /// a FirestoreEvent.)
    /// </summary>
    public class Function : ICloudEventFunction<StorageObjectData>
    {
        /// <summary>
        /// Logic for your function goes here. Note that a CloudEvent function just consumes an event;
        /// it doesn't provide any response.
        /// </summary>
        /// <param name="cloudEvent">The CloudEvent your function should consume.</param>
        /// <param name="data">The deserialized data within the CloudEvent.</param>
        /// <param name="cancellationToken">A cancellation token that is notified if the request is aborted.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task HandleAsync(CloudEvent cloudEvent, StorageObjectData data, CancellationToken cancellationToken)
        {
            Console.WriteLine("Storage object information:");
            Console.WriteLine($"  Name: {data.Name}");
            Console.WriteLine($"  Bucket: {data.Bucket}");
            Console.WriteLine($"  Size: {data.Size}");
            Console.WriteLine($"  Content type: {data.ContentType}");
            Console.WriteLine("CloudEvent information:");
            Console.WriteLine($"  ID: {cloudEvent.Id}");
            Console.WriteLine($"  Source: {cloudEvent.Source}");
            Console.WriteLine($"  Type: {cloudEvent.Type}");
            Console.WriteLine($"  Subject: {cloudEvent.Subject}");
            Console.WriteLine($"  DataSchema: {cloudEvent.DataSchema}");
            Console.WriteLine($"  DataContentType: {cloudEvent.DataContentType}");
            Console.WriteLine($"  Time: {cloudEvent.Time?.ToUniversalTime():yyyy-MM-dd'T'HH:mm:ss.fff'Z'}");
            Console.WriteLine($"  SpecVersion: {cloudEvent.SpecVersion}");
            
            // In this example, we don't need to perform any asynchronous operations, so the
            // method doesn't need to be declared async.
            return Task.CompletedTask;
        }
    }
}
