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

using Google.Cloud.Functions.Framework;
using Google.Cloud.Functions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;
using System.Threading.Tasks;

namespace Google.Cloud.Functions.Examples.DynamicFunction
{
    /// <summary>
    /// Startup to configure the system clock for NodaTime
    /// </summary>
    public class Startup : FunctionsStartup
    {
        public override void ConfigureServices(WebHostBuilderContext context, IServiceCollection services)
        {
            services.AddSingleton<IClock>(SystemClock.Instance);
        }
    }

    /// <summary>
    /// A dynamic function demonstrating injection of an HttpContext, logger and custom dependency (IClock)
    /// </summary>
    [FunctionsStartup(typeof(Startup))]
    public class Function : IDynamicFunction
    {
        public async Task HandleAsync(HttpContext context, ILogger logger, IClock clock)
        {
            logger.LogInformation("This is a log entry.");
            await context.Response.WriteAsync($"Hello, Functions Framework. The time is {clock.GetCurrentInstant()}.");
        }
    }
}
