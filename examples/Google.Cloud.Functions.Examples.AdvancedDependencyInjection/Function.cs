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
using Google.Cloud.Functions.Invoker.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text.Json;
using System.Threading.Tasks;

// FunctionsStartupAttribute is applied to the assembly to tell the Functions Framework which startup
// types to load. If you have multiple startup classes, you can apply the attribute multiple times.
// The attribute must be applied to the assembly containing the function, but it can potentially refer
// to a startup class in a different assembly.
[assembly:FunctionsStartup(typeof(Google.Cloud.Functions.Examples.AdvancedDependencyInjection.Startup))]

namespace Google.Cloud.Functions.Examples.AdvancedDependencyInjection
{
    // Dependency interfaces and implementation used in the Microsoft ASP.NET Core dependency injection documentation
    // at https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection
    public interface IOperation
    {
        Guid OperationId { get; }
    }

    public interface IOperationScoped : IOperation
    {
    }

    public interface IOperationSingleton : IOperation
    {
    }

    /// <summary>
    /// Implementation of both IOperationScoped and IOperationSingleton that
    /// just uses a new Guid for each instance's OperationId. (But a single instance will keep
    /// a stable OperationId for its whole lifetime.)
    /// </summary>
    public sealed class Operation : IOperationScoped, IOperationSingleton
    {
        public Guid OperationId { get; } = Guid.NewGuid();
    }

    /// <summary>
    /// The startup class is provided with a host builder which exposes a service collection.
    /// This can be used to make additional dependencies available.
    /// </summary>
    public class Startup : FunctionsStartup
    {
        // Provide implementations for IOperationSingleton, and IOperationScoped.
        // The implementation is the same for both interfaces (the Operation class)
        public override void Configure(IFunctionsHostBuilder builder) =>
            builder.Services
                .AddSingleton<IOperationSingleton, Operation>()
                .AddScoped<IOperationScoped, Operation>();
    }

    /// <summary>
    /// The actual Cloud Function using the dependencies.
    /// This uses both IOperationSingleton and IOperationScoped, and outputs the OperationId
    /// from each of them in HandleAsync. The service configuration provided in Startup
    /// means that for any given server, the SingletonOperationId property remains the same, but
    /// ScopedOperationId varies for each request.
    /// (Restarting the server will result in a different SingletonOperationId - nothing is persisted
    /// here.)
    /// </summary>
    public class Function : IHttpFunction
    {
        private static readonly JsonSerializerOptions s_indentedWriterOptions = new JsonSerializerOptions { WriteIndented = true };

        private readonly IOperationSingleton _singleton;
        private readonly IOperationScoped _scoped;

        public Function(IOperationSingleton singleton, IOperationScoped scoped) =>
            (_singleton, _scoped) = (singleton, scoped);

        public async Task HandleAsync(HttpContext context)
        {
            var output = new
            {
                SingletonOperationId = _singleton.OperationId,
                ScopedOperationId = _scoped.OperationId
            };

            context.Response.ContentType = "text/json";
            await JsonSerializer.SerializeAsync(context.Response.Body, output, options: s_indentedWriterOptions);
        }
    }
}
