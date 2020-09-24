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

using Google.Cloud.Functions.Hosting;
using Google.Cloud.Storage.V1;
using Google.Cloud.Vision.V1;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Google.Cloud.Functions.Examples.StorageImageAnnotator.Startup))]

namespace Google.Cloud.Functions.Examples.StorageImageAnnotator
{
    /// <summary>
    /// Startup class to provide the Storage and Vision API clients via dependency injection.
    /// We use singleton instances as the clients are thread-safe, and this ensures
    /// an efficient use of connections.
    /// </summary>
    public class Startup : FunctionsStartup
    {
        public override void ConfigureServices(WebHostBuilderContext context, IServiceCollection services) =>
            services
                .AddSingleton(ImageAnnotatorClient.Create())
                .AddSingleton(StorageClient.Create());
    }
}
