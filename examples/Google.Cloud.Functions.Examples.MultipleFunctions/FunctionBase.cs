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
using System;
using System.Threading.Tasks;

namespace Google.Cloud.Functions.Examples.MultipleFunctions
{
    /// <summary>
    /// Startup class registered for the (abstract) <see cref="FunctionBase"/> type.
    /// </summary>
    public class FunctionBaseStartup : FunctionsStartup
    {
        public override void ConfigureServices(WebHostBuilderContext context, IServiceCollection services) =>
            services.AddSingleton(new Dependency<decimal>(15.123m));
    }

    [FunctionsStartup(typeof(FunctionBaseStartup))]
    public abstract class FunctionBase : IHttpFunction
    {
        public Dependency<int> Int32Dependency { get; set; }
        public Dependency<string> StringDependency { get; set; }
        public Dependency<Guid> GuidDependency { get; set; }
        public Dependency<decimal> DecimalDependency { get; set; }

        public FunctionBase(Dependency<int> dep1, Dependency<string> dep2, Dependency<Guid> dep3, Dependency<decimal> dep4) =>
            (Int32Dependency, StringDependency, GuidDependency, DecimalDependency) = (dep1, dep2, dep3, dep4);

        public async Task HandleAsync(HttpContext context)
        {
            var response = context.Response;
            await response.WriteAsync($"Int32Dependency: {Int32Dependency?.Value.ToString() ?? "not set"}\n");
            await response.WriteAsync($"StringDependency: {StringDependency?.Value ?? "not set"}\n");
            await response.WriteAsync($"GuidDependency: {GuidDependency?.Value.ToString() ?? "not set"}\n");
            await response.WriteAsync($"DecimalDependency: {DecimalDependency?.Value.ToString() ?? "not set"}\n");
        }
    }
}
