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
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

[assembly: FunctionsStartup(typeof(Google.Cloud.Functions.Examples.TestableDependencies.Startup))]

namespace Google.Cloud.Functions.Examples.TestableDependencies
{
    // The dependency interface required by the function.
    public interface IDependency
    {
        public string Name { get; }
    }

    // The production dependency, configured in the Startup class.
    public class ProductionDependency : IDependency
    {
        public string Name => "Production dependency. Don't use me in tests!";
    }

    /// <summary>
    /// Adds the production dependency to the service collection.
    /// </summary>
    public class Startup : FunctionsStartup
    {
        public override void ConfigureServices(WebHostBuilderContext context, IServiceCollection services) =>
            services.AddSingleton<IDependency, ProductionDependency>();
    }

    public class Function : IHttpFunction
    {
        private readonly IDependency _dependency;

        public Function(IDependency dependency) =>
            _dependency = dependency;

        public async Task HandleAsync(HttpContext context)
        {
            await context.Response.WriteAsync($"Dependency configured for function: {_dependency.Name}");
        }
    }
}
