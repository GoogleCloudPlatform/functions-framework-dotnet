// Copyright 2021, Google LLC
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
using Google.Cloud.Functions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using ThirdPartyPackage1;
using ThirdPartyPackage2;

namespace CustomPayload3
{
    [FunctionsStartup(typeof(CloudEventFormatterStartup))]
    public class Function : ICloudEventFunction<Payload>
    {
        public Task HandleAsync(CloudEvent cloudEvent, Payload data, CancellationToken cancellationToken)
        {
            Console.WriteLine($"ID: {cloudEvent.Id}");
            Console.WriteLine($"Text: {data.Text}");
            Console.WriteLine($"Number: {data.Number}");
            return Task.CompletedTask;
        }
    }
}
