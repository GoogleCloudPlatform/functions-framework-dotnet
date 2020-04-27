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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Google.Cloud.Functions.Framework
{
    /// <summary>
    /// Just a sample of a Cloud Event Publisher - this just writes to the console.
    /// </summary>
    public class DemoCloudEventPublisher : ICloudEventPublisher
    {
        /// <inheritdoc />
        public Task PublishAsync(CloudEvent cloudEvent, CancellationToken cancellationToken)
        {
            var formatter = new JsonEventFormatter();
            byte[] data = formatter.EncodeStructuredEvent(cloudEvent, out _);
            Console.WriteLine($"Publishing Cloud Event {cloudEvent.Id}:");
            Console.WriteLine(Encoding.UTF8.GetString(data));
            return Task.CompletedTask;
        }
    }
}
