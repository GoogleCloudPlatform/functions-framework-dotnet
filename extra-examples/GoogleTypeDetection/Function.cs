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
using Google.Events.Protobuf.Cloud.PubSub.V1;
using Google.Events.Protobuf.Cloud.Storage.V1;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GoogleTypeDetection
{
    public class Startup : FunctionsStartup
    {
        public override void ConfigureServices(WebHostBuilderContext context, IServiceCollection services) =>
            services.AddSingleton<CloudEventFormatter, GoogleEventFormatter>();
    }

    [FunctionsStartup(typeof(Startup))]
    public class Function : ICloudEventFunction
    {
        public Task HandleAsync(CloudEvent cloudEvent, CancellationToken cancellationToken)
        {
            Console.WriteLine($"ID: {cloudEvent.Id}");
            switch (cloudEvent.Data)
            {
                case StorageObjectData storage:
                    Console.WriteLine($"Storage Bucket: {storage.Bucket}");
                    break;
                case MessagePublishedData pubsub:
                    Console.WriteLine($"PubSub subscription: {pubsub.Subscription}");
                    break;
                default:
                    Console.WriteLine($"Unhandled event type {cloudEvent.Type}");
                    break;
            }
            return Task.CompletedTask;
        }
    }
}
