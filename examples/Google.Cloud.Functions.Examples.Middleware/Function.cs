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
using Google.Cloud.Functions.Invoker;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;

// FunctionsServiceProviderAttribute is applied to the assembly to tell the Functions Framework which startup class
// to load. If you have multiple startup classes, you can apply the attribute multiple times, optionally
// using the Order property to specify ordering.
// The attribute must be applied to the assembly containing the function, but it can potentially refer
// to a startup class in a different assembly.
[assembly: FunctionsStartup(typeof(Google.Cloud.Functions.Examples.Middleware.Startup))]

namespace Google.Cloud.Functions.Examples.Middleware
{
    /// <summary>
    /// The startup class can be used to perform additional configuration, including
    /// adding application configuration sources, reconfiguring logging, providing services
    /// for dependency injection, and adding middleware to the eventual application pipeline.
    /// In this case, we add a simple piece of middleware to the request pipeline.
    /// </summary>
    public class Startup : FunctionsStartup
    {
        public override void Configure(WebHostBuilderContext context, IApplicationBuilder app) =>
            app.UseMiddleware<SampleMiddleware>();
    }

    /// <summary>
    /// This middleware just provides a single log entry per successful request.
    /// (This is not terribly useful as middleware, but it demonstrates the concept simply.)
    /// </summary>
    public class SampleMiddleware
    {
        private readonly RequestDelegate _next;

        public SampleMiddleware(RequestDelegate next) =>
            _next = next;

        public async Task InvokeAsync(HttpContext context, ILogger<SampleMiddleware> logger)
        {
            Stopwatch sw = Stopwatch.StartNew();
            await _next(context);
            sw.Stop();
            logger.LogInformation("Path: {path}; Status: {status}; Time: {time}ms",
                context.Request.Path, context.Response.StatusCode, sw.Elapsed.TotalMilliseconds);
        }
    }

    /// <summary>
    /// The actual Cloud Function.
    /// </summary>
    public class Function : IHttpFunction
    {
        public async Task HandleAsync(HttpContext context) =>
            await context.Response.WriteAsync("Response to be logged by middleware.");
    }
}
